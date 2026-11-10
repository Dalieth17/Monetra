package com.monetra.app.ui.debts

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
import androidx.compose.material.icons.filled.Payments
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Card
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.FilterChip
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
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
import com.monetra.app.data.local.entities.DebtEntity
import com.monetra.app.ui.common.DatePickerField
import com.monetra.app.ui.common.EmptyState
import com.monetra.app.ui.common.ErrorBanner
import java.math.BigDecimal

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DebtsScreen(
    onNavigateBack: () -> Unit,
    viewModel: DebtsViewModel = viewModel(factory = DebtsViewModel.Factory)
) {
    val state by viewModel.uiState.collectAsState()
    val debts by viewModel.debts.collectAsState()
    val snackbarHostState = remember { SnackbarHostState() }
    var showCreateDialog by remember { mutableStateOf(false) }
    var paymentTarget by remember { mutableStateOf<DebtEntity?>(null) }

    LaunchedEffect(state.infoMessage) {
        state.infoMessage?.let {
            snackbarHostState.showSnackbar(it)
            viewModel.dismissMessages()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Mis Deudas") },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        },
        floatingActionButton = {
            FloatingActionButton(onClick = { showCreateDialog = true }) {
                Icon(Icons.Default.Add, contentDescription = "Registrar deuda")
            }
        },
        snackbarHost = { SnackbarHost(snackbarHostState) }
    ) { padding ->
        Column(modifier = Modifier.padding(padding).fillMaxSize()) {
            state.errorMessage?.let { ErrorBanner(message = it, onRetry = viewModel::refresh) }

            if (debts.isEmpty()) {
                EmptyState("No tienes deudas registradas. ¡Excelente salud financiera!")
            } else {
                LazyColumn(
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    items(debts, key = { it.id }) { debt ->
                        DebtCard(
                            debt = debt,
                            onPay = { paymentTarget = debt },
                            onDelete = { viewModel.deleteDebt(debt.id) }
                        )
                    }
                }
            }
        }
    }

    if (showCreateDialog) {
        CreateDebtDialog(
            onDismiss = { showCreateDialog = false },
            onConfirm = { creditor, amount, rate, rateType, dueDate ->
                viewModel.createDebt(creditor, amount, rate, rateType, dueDate)
                showCreateDialog = false
            }
        )
    }

    paymentTarget?.let { debt ->
        RecordPaymentDialog(
            debt = debt,
            accounts = state.accounts.map { it.id to "${it.name} (${it.currencyCode})" },
            onDismiss = { paymentTarget = null },
            onConfirm = { accountId, amount ->
                viewModel.recordPayment(debt.id, accountId, amount)
                paymentTarget = null
            }
        )
    }
}

@Composable
private fun DebtCard(debt: DebtEntity, onPay: () -> Unit, onDelete: () -> Unit) {
    val progress = if (debt.totalAmount > BigDecimal.ZERO) {
        1f - (debt.remainingAmount.toFloat() / debt.totalAmount.toFloat())
    } else 0f

    Card(modifier = Modifier.fillMaxWidth()) {
        Column(Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Column(Modifier.weight(1f)) {
                    Text(debt.creditor, style = MaterialTheme.typography.titleLarge)
                    Text(
                        "Tasa ${debt.interestRate}% ${debt.rateType} · Vence ${DateUtils.millisToDisplay(debt.dueDate)}",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                    )
                }
                IconButton(onClick = onPay) {
                    Icon(Icons.Default.Payments, contentDescription = "Abonar", tint = MaterialTheme.colorScheme.primary)
                }
                IconButton(onClick = onDelete) {
                    Icon(Icons.Default.Delete, contentDescription = "Eliminar", tint = MaterialTheme.colorScheme.error)
                }
            }
            Spacer(Modifier.height(8.dp))
            LinearProgressIndicator(
                progress = { progress.coerceIn(0f, 1f) },
                modifier = Modifier.fillMaxWidth()
            )
            Spacer(Modifier.height(8.dp))
            Text(
                "Saldo: ${CurrencyFormatter.format(debt.remainingAmount, "COP")} de ${CurrencyFormatter.format(debt.totalAmount, "COP")}",
                style = MaterialTheme.typography.bodyLarge
            )
            Text(
                "Cuota mensual estimada: ${CurrencyFormatter.format(debt.estimatedMonthlyPayment, "COP")}",
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
            )
        }
    }
}

@Composable
private fun CreateDebtDialog(
    onDismiss: () -> Unit,
    onConfirm: (String, BigDecimal, BigDecimal, String, Long) -> Unit
) {
    var creditor by remember { mutableStateOf("") }
    var amountText by remember { mutableStateOf("") }
    var rateText by remember { mutableStateOf("") }
    var rateType by remember { mutableStateOf("EA") }
    var dueDate by remember { mutableStateOf<Long?>(null) }

    val amount = amountText.toBigDecimalOrNull()
    val rate = rateText.toBigDecimalOrNull()
    val isValid = creditor.isNotBlank() && amount != null && amount > BigDecimal.ZERO &&
        rate != null && rate >= BigDecimal.ZERO && dueDate != null

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Registrar Deuda") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = creditor,
                    onValueChange = { creditor = it },
                    label = { Text("Entidad acreedora") },
                    singleLine = true
                )
                OutlinedTextField(
                    value = amountText,
                    onValueChange = { amountText = it },
                    label = { Text("Capital inicial") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                    singleLine = true
                )
                OutlinedTextField(
                    value = rateText,
                    onValueChange = { rateText = it },
                    label = { Text("Tasa de interés (%)") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                    singleLine = true
                )
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    FilterChip(selected = rateType == "EA", onClick = { rateType = "EA" }, label = { Text("EA") })
                    FilterChip(selected = rateType == "EM", onClick = { rateType = "EM" }, label = { Text("EM") })
                }
                DatePickerField(
                    label = "Fecha de vencimiento",
                    selectedMillis = dueDate,
                    onDateSelected = { dueDate = it }
                )
            }
        },
        confirmButton = {
            TextButton(
                enabled = isValid,
                onClick = { onConfirm(creditor.trim(), amount!!, rate!!, rateType, dueDate!!) }
            ) { Text("Guardar") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Cancelar") } }
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun RecordPaymentDialog(
    debt: DebtEntity,
    accounts: List<Pair<Int, String>>,
    onDismiss: () -> Unit,
    onConfirm: (Int, BigDecimal) -> Unit
) {
    var amountText by remember { mutableStateOf("") }
    var selectedAccount by remember { mutableStateOf(accounts.firstOrNull()) }
    var menuExpanded by remember { mutableStateOf(false) }

    val amount = amountText.toBigDecimalOrNull()
    val isValid = amount != null && amount > BigDecimal.ZERO && selectedAccount != null

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Abonar a ${debt.creditor}") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = amountText,
                    onValueChange = { amountText = it },
                    label = { Text("Monto del abono") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                    singleLine = true
                )
                ExposedDropdownMenuBox(expanded = menuExpanded, onExpandedChange = { menuExpanded = it }) {
                    OutlinedTextField(
                        value = selectedAccount?.second ?: "Selecciona una cuenta",
                        onValueChange = {},
                        readOnly = true,
                        label = { Text("Cuenta origen") },
                        trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = menuExpanded) },
                        modifier = Modifier.menuAnchor()
                    )
                    ExposedDropdownMenu(expanded = menuExpanded, onDismissRequest = { menuExpanded = false }) {
                        accounts.forEach { account ->
                            DropdownMenuItem(
                                text = { Text(account.second) },
                                onClick = {
                                    selectedAccount = account
                                    menuExpanded = false
                                }
                            )
                        }
                    }
                }
            }
        },
        confirmButton = {
            TextButton(
                enabled = isValid,
                onClick = { onConfirm(selectedAccount!!.first, amount!!) }
            ) { Text("Abonar") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Cancelar") } }
    )
}

private fun String.toBigDecimalOrNull(): BigDecimal? = try {
    if (isBlank()) null else BigDecimal(replace(",", "."))
} catch (_: NumberFormatException) {
    null
}
