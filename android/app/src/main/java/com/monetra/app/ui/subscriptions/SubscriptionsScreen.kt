package com.monetra.app.ui.subscriptions

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.NotificationsActive
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Card
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FilterChip
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.monetra.app.core.CurrencyFormatter
import com.monetra.app.core.DateUtils
import com.monetra.app.data.local.entities.SubscriptionEntity
import com.monetra.app.ui.common.DatePickerField
import com.monetra.app.ui.common.EmptyState
import com.monetra.app.ui.common.ErrorBanner
import com.monetra.app.ui.theme.WarningAmber
import java.math.BigDecimal

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SubscriptionsScreen(
    onNavigateBack: () -> Unit,
    viewModel: SubscriptionsViewModel = viewModel(factory = SubscriptionsViewModel.Factory)
) {
    val state by viewModel.uiState.collectAsState()
    val subscriptions by viewModel.subscriptions.collectAsState()
    var showCreateDialog by remember { mutableStateOf(false) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Suscripciones") },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        },
        floatingActionButton = {
            FloatingActionButton(onClick = { showCreateDialog = true }) {
                Icon(Icons.Default.Add, contentDescription = "Añadir suscripción")
            }
        }
    ) { padding ->
        Column(modifier = Modifier.padding(padding).fillMaxSize()) {
            state.errorMessage?.let {
                ErrorBanner(message = it, onRetry = { viewModel.dismissError(); viewModel.refresh() })
            }

            if (subscriptions.isEmpty()) {
                EmptyState("No tienes suscripciones registradas. Mapea tus gastos fijos con el botón +")
            } else {
                LazyColumn(
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    items(subscriptions, key = { it.id }) { subscription ->
                        SubscriptionCard(
                            subscription = subscription,
                            onToggle = { viewModel.toggle(subscription.id) },
                            onDelete = { viewModel.delete(subscription.id) }
                        )
                    }
                }
            }
        }
    }

    if (showCreateDialog) {
        CreateSubscriptionDialog(
            onDismiss = { showCreateDialog = false },
            onConfirm = { name, amount, currency, cycle, nextPayment ->
                viewModel.create(name, amount, currency, cycle, nextPayment)
                showCreateDialog = false
            }
        )
    }
}

@Composable
private fun SubscriptionCard(
    subscription: SubscriptionEntity,
    onToggle: () -> Unit,
    onDelete: () -> Unit
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Column(Modifier.weight(1f)) {
                    Text(subscription.name, style = MaterialTheme.typography.titleLarge)
                    Text(
                        "${subscription.billingCycle} · Próximo cobro: ${DateUtils.millisToDisplay(subscription.nextPaymentDate)}",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                    )
                }
                Switch(checked = subscription.isActive, onCheckedChange = { onToggle() })
                IconButton(onClick = onDelete) {
                    Icon(Icons.Default.Delete, contentDescription = "Eliminar", tint = MaterialTheme.colorScheme.error)
                }
            }
            Spacer(Modifier.height(8.dp))
            Text(
                CurrencyFormatter.format(subscription.amount, subscription.currencyCode),
                style = MaterialTheme.typography.bodyLarge
            )
            // Alerta previa a la fecha de cobro (criterio de aceptación F4 del PRD)
            if (subscription.isActive && subscription.daysUntilPayment <= 3) {
                Spacer(Modifier.height(6.dp))
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        Icons.Default.NotificationsActive,
                        contentDescription = null,
                        tint = WarningAmber,
                        modifier = Modifier.padding(end = 6.dp)
                    )
                    Text(
                        if (subscription.daysUntilPayment == 0) "¡El cobro es hoy!"
                        else "Cobro en ${subscription.daysUntilPayment} día(s)",
                        style = MaterialTheme.typography.labelSmall,
                        color = WarningAmber
                    )
                }
            }
        }
    }
}

@Composable
private fun CreateSubscriptionDialog(
    onDismiss: () -> Unit,
    onConfirm: (String, BigDecimal, String, String, Long) -> Unit
) {
    var name by remember { mutableStateOf("") }
    var amountText by remember { mutableStateOf("") }
    var currency by remember { mutableStateOf("COP") }
    var cycle by remember { mutableStateOf("Monthly") }
    var nextPayment by remember { mutableStateOf<Long?>(null) }

    val amount = amountText.toBigDecimalOrNull()
    val isValid = name.isNotBlank() && amount != null && amount > BigDecimal.ZERO && nextPayment != null

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Nueva Suscripción") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = name,
                    onValueChange = { name = it },
                    label = { Text("Nombre (ej. Netflix)") },
                    singleLine = true
                )
                OutlinedTextField(
                    value = amountText,
                    onValueChange = { amountText = it },
                    label = { Text("Monto") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                    singleLine = true
                )
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    FilterChip(selected = currency == "COP", onClick = { currency = "COP" }, label = { Text("COP") })
                    FilterChip(selected = currency == "USD", onClick = { currency = "USD" }, label = { Text("USD") })
                }
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    FilterChip(selected = cycle == "Weekly", onClick = { cycle = "Weekly" }, label = { Text("Semanal") })
                    FilterChip(selected = cycle == "Monthly", onClick = { cycle = "Monthly" }, label = { Text("Mensual") })
                    FilterChip(selected = cycle == "Annual", onClick = { cycle = "Annual" }, label = { Text("Anual") })
                }
                DatePickerField(
                    label = "Próxima fecha de cobro",
                    selectedMillis = nextPayment,
                    onDateSelected = { nextPayment = it }
                )
            }
        },
        confirmButton = {
            TextButton(
                enabled = isValid,
                onClick = { onConfirm(name.trim(), amount!!, currency, cycle, nextPayment!!) }
            ) { Text("Guardar") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Cancelar") } }
    )
}

private fun String.toBigDecimalOrNull(): BigDecimal? = try {
    if (isBlank()) null else BigDecimal(replace(",", "."))
} catch (_: NumberFormatException) {
    null
}
