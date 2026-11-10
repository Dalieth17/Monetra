package com.monetra.app.ui.savings

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
import androidx.compose.material.icons.filled.Celebration
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Savings
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Card
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
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
import com.monetra.app.data.local.entities.SavingsGoalEntity
import com.monetra.app.ui.common.DatePickerField
import com.monetra.app.ui.common.EmptyState
import com.monetra.app.ui.common.ErrorBanner
import java.math.BigDecimal

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SavingsGoalsScreen(
    onNavigateBack: () -> Unit,
    viewModel: SavingsGoalsViewModel = viewModel(factory = SavingsGoalsViewModel.Factory)
) {
    val state by viewModel.uiState.collectAsState()
    val goals by viewModel.goals.collectAsState()
    var showCreateDialog by remember { mutableStateOf(false) }
    var depositTarget by remember { mutableStateOf<SavingsGoalEntity?>(null) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Metas de Ahorro") },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        },
        floatingActionButton = {
            FloatingActionButton(onClick = { showCreateDialog = true }) {
                Icon(Icons.Default.Add, contentDescription = "Crear meta")
            }
        }
    ) { padding ->
        Column(modifier = Modifier.padding(padding).fillMaxSize()) {
            state.errorMessage?.let { ErrorBanner(message = it, onRetry = { viewModel.dismissError(); viewModel.refresh() }) }

            if (goals.isEmpty()) {
                EmptyState("No tienes metas de ahorro activas. Crea una con el botón +")
            } else {
                LazyColumn(
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    items(goals, key = { it.id }) { goal ->
                        GoalCard(
                            goal = goal,
                            onDeposit = { depositTarget = goal },
                            onDelete = { viewModel.deleteGoal(goal.id) }
                        )
                    }
                }
            }
        }
    }

    if (showCreateDialog) {
        CreateGoalDialog(
            onDismiss = { showCreateDialog = false },
            onConfirm = { name, target, deadline ->
                viewModel.createGoal(name, target, deadline)
                showCreateDialog = false
            }
        )
    }

    depositTarget?.let { goal ->
        DepositDialog(
            goal = goal,
            accounts = state.accounts.map { it.id to "${it.name} (${it.currencyCode})" },
            onDismiss = { depositTarget = null },
            onConfirm = { accountId, amount ->
                viewModel.deposit(goal.id, accountId, amount)
                depositTarget = null
            }
        )
    }

    // Tarjeta de felicitación al completar una meta (cierre de bucle del documento de UX)
    state.completedGoalName?.let { goalName ->
        AlertDialog(
            onDismissRequest = viewModel::dismissCelebration,
            icon = { Icon(Icons.Default.Celebration, contentDescription = null, tint = MaterialTheme.colorScheme.primary) },
            title = { Text("¡Meta completada!") },
            text = { Text("Felicitaciones, alcanzaste tu meta \"$goalName\". Tu disciplina financiera está dando frutos.") },
            confirmButton = {
                TextButton(onClick = viewModel::dismissCelebration) { Text("Ver Progreso") }
            }
        )
    }
}

@Composable
private fun GoalCard(goal: SavingsGoalEntity, onDeposit: () -> Unit, onDelete: () -> Unit) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Column(Modifier.weight(1f)) {
                    Text(goal.name, style = MaterialTheme.typography.titleLarge)
                    Text(
                        "Fecha límite: ${DateUtils.millisToDisplay(goal.deadline)}",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                    )
                }
                IconButton(onClick = onDeposit) {
                    Icon(Icons.Default.Savings, contentDescription = "Depositar", tint = MaterialTheme.colorScheme.primary)
                }
                IconButton(onClick = onDelete) {
                    Icon(Icons.Default.Delete, contentDescription = "Eliminar", tint = MaterialTheme.colorScheme.error)
                }
            }
            Spacer(Modifier.height(8.dp))
            LinearProgressIndicator(
                progress = { (goal.progressPercentage / 100.0).toFloat().coerceIn(0f, 1f) },
                modifier = Modifier.fillMaxWidth()
            )
            Spacer(Modifier.height(8.dp))
            Text(
                "${CurrencyFormatter.format(goal.currentAmount, "COP")} de ${CurrencyFormatter.format(goal.targetAmount, "COP")} (${goal.progressPercentage.toInt()}%)",
                style = MaterialTheme.typography.bodyLarge
            )
        }
    }
}

@Composable
private fun CreateGoalDialog(
    onDismiss: () -> Unit,
    onConfirm: (String, BigDecimal, Long) -> Unit
) {
    var name by remember { mutableStateOf("") }
    var targetText by remember { mutableStateOf("") }
    var deadline by remember { mutableStateOf<Long?>(null) }

    val target = targetText.toBigDecimalOrNull()
    val isValid = name.isNotBlank() && target != null && target > BigDecimal.ZERO && deadline != null

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Nueva Meta de Ahorro") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = name,
                    onValueChange = { name = it },
                    label = { Text("Nombre (ej. Viaje a San Andrés)") },
                    singleLine = true
                )
                OutlinedTextField(
                    value = targetText,
                    onValueChange = { targetText = it },
                    label = { Text("Monto objetivo") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                    singleLine = true
                )
                DatePickerField(
                    label = "Fecha límite",
                    selectedMillis = deadline,
                    onDateSelected = { deadline = it }
                )
            }
        },
        confirmButton = {
            TextButton(
                enabled = isValid,
                onClick = { onConfirm(name.trim(), target!!, deadline!!) }
            ) { Text("Crear") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Cancelar") } }
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun DepositDialog(
    goal: SavingsGoalEntity,
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
        title = { Text("Depositar en \"${goal.name}\"") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = amountText,
                    onValueChange = { amountText = it },
                    label = { Text("Monto a depositar") },
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
            ) { Text("Depositar") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Cancelar") } }
    )
}

private fun String.toBigDecimalOrNull(): BigDecimal? = try {
    if (isBlank()) null else BigDecimal(replace(",", "."))
} catch (_: NumberFormatException) {
    null
}
