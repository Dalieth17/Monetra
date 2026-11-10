package com.monetra.app.data.repository

import com.monetra.app.core.DateUtils
import com.monetra.app.data.local.SessionManager
import com.monetra.app.data.local.dao.AccountDao
import com.monetra.app.data.local.entities.AccountEntity
import com.monetra.app.data.local.entities.TransactionEntity
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.safeApiCall
import java.math.BigDecimal
import java.math.RoundingMode

/** Modelo de dominio del dashboard, servible desde red o desde Room (offline). */
data class DashboardData(
    val baseCurrency: String,
    val totalNetWorth: BigDecimal,
    val monthlyIncome: BigDecimal,
    val monthlyExpense: BigDecimal,
    val exchangeRate: BigDecimal,
    val rateTimestampMillis: Long,
    val isRateCached: Boolean,
    val isOffline: Boolean,
    val accounts: List<AccountEntity>,
    val recentTransactions: List<TransactionEntity>
)

class DashboardRepository(
    private val api: MonetraApi,
    private val accountDao: AccountDao,
    private val transactionRepository: TransactionRepository,
    private val sessionManager: SessionManager
) {

    suspend fun getDashboard(baseCurrency: String): Result<DashboardData> {
        val result = safeApiCall { api.getDashboard(baseCurrency) }

        result.onSuccess { dto ->
            val accounts = dto.accounts.map { it.toEntity() }
            accountDao.upsertAll(accounts)
            sessionManager.saveLastExchangeRate(
                dto.usdToCopExchangeRate,
                DateUtils.parseIsoToMillis(dto.exchangeRateTimestamp)
            )
            return Result.success(
                DashboardData(
                    baseCurrency = dto.baseCurrency,
                    totalNetWorth = dto.totalNetWorth,
                    monthlyIncome = dto.monthlyIncome,
                    monthlyExpense = dto.monthlyExpense,
                    exchangeRate = dto.usdToCopExchangeRate,
                    rateTimestampMillis = DateUtils.parseIsoToMillis(dto.exchangeRateTimestamp),
                    isRateCached = dto.isRateCached,
                    isOffline = false,
                    accounts = accounts,
                    recentTransactions = dto.recentTransactions.map { it.toEntity() }
                )
            )
        }

        return Result.success(buildOfflineDashboard(baseCurrency))
    }

    /**
     * Modo offline: patrimonio calculado desde Room usando la última tasa cacheada,
     * con advertencia visual de desactualización (edge case del PRD).
     */
    private suspend fun buildOfflineDashboard(baseCurrency: String): DashboardData {
        val accounts = accountDao.getAll()
        val rate = sessionManager.lastExchangeRate() ?: BigDecimal.ZERO
        val (income, expense) = transactionRepository.getLocalMonthlySummary()

        var netWorth = BigDecimal.ZERO
        accounts.forEach { account ->
            netWorth = netWorth.add(convert(account.currentBalance, account.currencyCode, baseCurrency, rate))
        }

        return DashboardData(
            baseCurrency = baseCurrency,
            totalNetWorth = netWorth.setScale(2, RoundingMode.HALF_UP),
            monthlyIncome = income,
            monthlyExpense = expense,
            exchangeRate = rate,
            rateTimestampMillis = sessionManager.lastExchangeRateAt(),
            isRateCached = true,
            isOffline = true,
            accounts = accounts,
            recentTransactions = emptyList()
        )
    }

    private fun convert(amount: BigDecimal, from: String, to: String, usdToCop: BigDecimal): BigDecimal {
        if (from == to) return amount
        if (usdToCop <= BigDecimal.ZERO) return amount
        return when {
            from == "USD" && to == "COP" -> amount.multiply(usdToCop)
            from == "COP" && to == "USD" -> amount.divide(usdToCop, 2, RoundingMode.HALF_UP)
            else -> amount
        }
    }
}
