package com.monetra.app.data.remote.dto

import java.math.BigDecimal

data class DashboardSummaryDto(
    val baseCurrency: String,
    val totalNetWorth: BigDecimal,
    val monthlyIncome: BigDecimal,
    val monthlyExpense: BigDecimal,
    val netSavingsThisMonth: BigDecimal,
    val usdToCopExchangeRate: BigDecimal,
    val exchangeRateTimestamp: String,
    val isRateCached: Boolean,
    val accounts: List<AccountDto>,
    val recentTransactions: List<TransactionDto>
)
