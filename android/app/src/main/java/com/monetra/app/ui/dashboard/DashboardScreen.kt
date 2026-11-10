package com.monetra.app.ui.dashboard

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material.icons.automirrored.filled.TrendingDown
import androidx.compose.material.icons.automirrored.filled.TrendingUp
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.CreditCard
import androidx.compose.material.icons.filled.Savings
import androidx.compose.material.icons.filled.Subscriptions
import androidx.compose.material.icons.filled.SwapHoriz
import androidx.compose.material.icons.filled.SyncProblem
import androidx.compose.material3.AssistChip
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.monetra.app.core.CurrencyFormatter
import com.monetra.app.core.DateUtils
import com.monetra.app.data.local.entities.TransactionEntity
import com.monetra.app.ui.common.DashboardShimmer
import com.monetra.app.ui.common.EmptyState
import com.monetra.app.ui.common.ErrorBanner
import com.monetra.app.ui.common.OfflineBadge
import com.monetra.app.ui.theme.ExpenseRed
import com.monetra.app.ui.theme.IncomeGreen

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    onAddTransaction: () -> Unit,
    onOpenDebts: () -> Unit,
    onOpenSavings: () -> Unit,
    onOpenSubscriptions: () -> Unit,
    onLogout: () -> Unit,
    viewModel: DashboardViewModel = viewModel(factory = DashboardViewModel.Factory)
) {
    val state by viewModel.uiState.collectAsState()
    val pendingCount by viewModel.pendingSyncCount.collectAsState()
    val snackbarHostState = remember { SnackbarHostState() }

    LaunchedEffect(state.syncedMessage) {
        state.syncedMessage?.let {
            snackbarHostState.showSnackbar(it)
            viewModel.dismissSyncMessage()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Monetra") },
                actions = {
                    IconButton(onClick = viewModel::toggleBaseCurrency) {
                        Icon(Icons.Default.SwapHoriz, contentDescription = "Cambiar moneda base")
                    }
                    IconButton(onClick = { viewModel.logout(); onLogout() }) {
                        Icon(Icons.AutoMirrored.Filled.Logout, contentDescription = "Cerrar sesión")
                    }
                }
            )
        },
        floatingActionButton = {
            FloatingActionButton(onClick = onAddTransaction) {
                Icon(Icons.Default.Add, contentDescription = "Añadir Movimiento")
            }
        },
        snackbarHost = { SnackbarHost(snackbarHostState) }
    ) { padding ->
        Column(modifier = Modifier.padding(padding).fillMaxSize()) {

            state.errorMessage?.let { ErrorBanner(message = it, onRetry = viewModel::load) }

            if (state.isLoading) {
                DashboardShimmer()
                return@Column
            }

            val data = state.data ?: return@Column

            LazyColumn(
                modifier = Modifier.fillMaxSize(),
                contentPadding = androidx.compose.foundation.layout.PaddingValues(16.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                if (data.isOffline) {
                    item {
                        OfflineBadge("Modo offline: mostrando datos locales. Se sincronizará al reconectar.")
                    }
                } else if (data.isRateCached) {
                    item {
                        OfflineBadge(
                            "Tasa de cambio desactualizada (último valor: ${DateUtils.millisToDisplay(data.rateTimestampMillis)})"
                        )
                    }
                }

                if (pendingCount > 0) {
                    item {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Icon(
                                Icons.Default.SyncProblem,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.secondary,
                                modifier = Modifier.size(16.dp)
                            )
                            Spacer(Modifier.width(6.dp))
                            Text(
                                "$pendingCount movimiento(s) pendiente(s) de sincronizar",
                                style = MaterialTheme.typography.labelSmall
                            )
                        }
                    }
                }

                item {
                    NetWorthCard(
                        netWorth = CurrencyFormatter.format(data.totalNetWorth, data.baseCurrency),
                        baseCurrency = data.baseCurrency,
                        exchangeRate = data.exchangeRate.toPlainString()
                    )
                }

                item {
                    Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                        MonthlyMetricCard(
                            modifier = Modifier.weight(1f),
                            label = "Ingresos del mes",
                            amount = CurrencyFormatter.format(data.monthlyIncome, data.baseCurrency),
                            isIncome = true
                        )
                        MonthlyMetricCard(
                            modifier = Modifier.weight(1f),
                            label = "Egresos del mes",
                            amount = CurrencyFormatter.format(data.monthlyExpense, data.baseCurrency),
                            isIncome = false
                        )
                    }
                }

                item {
                    Text("Mis Cuentas", style = MaterialTheme.typography.titleLarge)
                }
                item {
                    LazyRow(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                        items(data.accounts, key = { it.id }) { account ->
                            Card(modifier = Modifier.width(180.dp)) {
                                Column(Modifier.padding(12.dp)) {
                                    Text(account.name, style = MaterialTheme.typography.labelSmall)
                                    Spacer(Modifier.height(4.dp))
                                    Text(
                                        CurrencyFormatter.format(account.currentBalance, account.currencyCode),
                                        style = MaterialTheme.typography.titleLarge
                                    )
                                }
                            }
                        }
                    }
                }

                item {
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        AssistChip(
                            onClick = onOpenDebts,
                            label = { Text("Deudas") },
                            leadingIcon = { Icon(Icons.Default.CreditCard, null, Modifier.size(18.dp)) }
                        )
                        AssistChip(
                            onClick = onOpenSavings,
                            label = { Text("Metas") },
                            leadingIcon = { Icon(Icons.Default.Savings, null, Modifier.size(18.dp)) }
                        )
                        AssistChip(
                            onClick = onOpenSubscriptions,
                            label = { Text("Suscripciones") },
                            leadingIcon = { Icon(Icons.Default.Subscriptions, null, Modifier.size(18.dp)) }
                        )
                    }
                }

                item {
                    Text("Movimientos Recientes", style = MaterialTheme.typography.titleLarge)
                }

                if (data.recentTransactions.isEmpty()) {
                    item {
                        EmptyState("No tienes movimientos registrados este mes. Toca el botón + para empezar")
                    }
                } else {
                    items(data.recentTransactions, key = { it.serverId ?: -it.localId }) { transaction ->
                        TransactionRow(transaction)
                    }
                }
            }
        }
    }
}

@Composable
private fun NetWorthCard(netWorth: String, baseCurrency: String, exchangeRate: String) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.secondary)
    ) {
        Column(Modifier.padding(20.dp)) {
            Text(
                "Patrimonio Neto ($baseCurrency)",
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSecondary.copy(alpha = 0.8f)
            )
            Spacer(Modifier.height(8.dp))
            Text(
                netWorth,
                style = MaterialTheme.typography.headlineMedium,
                color = MaterialTheme.colorScheme.onSecondary
            )
            Spacer(Modifier.height(8.dp))
            Text(
                "TRM USD/COP: $exchangeRate",
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSecondary.copy(alpha = 0.7f)
            )
        }
    }
}

@Composable
private fun MonthlyMetricCard(modifier: Modifier, label: String, amount: String, isIncome: Boolean) {
    Card(modifier = modifier) {
        Column(Modifier.padding(12.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(
                    if (isIncome) Icons.AutoMirrored.Filled.TrendingUp else Icons.AutoMirrored.Filled.TrendingDown,
                    contentDescription = null,
                    tint = if (isIncome) IncomeGreen else ExpenseRed,
                    modifier = Modifier.size(18.dp)
                )
                Spacer(Modifier.width(6.dp))
                Text(label, style = MaterialTheme.typography.labelSmall)
            }
            Spacer(Modifier.height(6.dp))
            Text(amount, style = MaterialTheme.typography.bodyLarge)
        }
    }
}

@Composable
fun TransactionRow(transaction: TransactionEntity) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Row(
            modifier = Modifier.padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(Modifier.weight(1f)) {
                Text(
                    transaction.description.ifEmpty { transaction.categoryName },
                    style = MaterialTheme.typography.bodyLarge
                )
                Text(
                    "${transaction.categoryName} · ${transaction.accountName} · ${DateUtils.millisToDisplay(transaction.date)}" +
                        if (!transaction.isSynced) " · Pendiente de sync" else "",
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                )
            }
            Text(
                (if (transaction.type == 1) "+" else "-") +
                    CurrencyFormatter.format(transaction.amount, transaction.currencyCode),
                style = MaterialTheme.typography.bodyLarge,
                color = if (transaction.type == 1) IncomeGreen else ExpenseRed
            )
        }
    }
}
