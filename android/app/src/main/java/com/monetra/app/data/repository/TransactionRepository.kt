package com.monetra.app.data.repository

import com.monetra.app.core.DateUtils
import com.monetra.app.core.NetworkMonitor
import com.monetra.app.data.local.dao.AccountDao
import com.monetra.app.data.local.dao.TransactionDao
import com.monetra.app.data.local.entities.TransactionEntity
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.OfflineException
import com.monetra.app.data.remote.dto.CategorySuggestionDto
import com.monetra.app.data.remote.dto.CreateTransactionRequest
import com.monetra.app.data.remote.safeApiCall
import kotlinx.coroutines.flow.Flow
import java.math.BigDecimal

/** Resultado de creación: indica si quedó guardada offline pendiente de sincronizar. */
data class CreateTransactionResult(val savedOffline: Boolean)

class TransactionRepository(
    private val api: MonetraApi,
    private val transactionDao: TransactionDao,
    private val accountDao: AccountDao,
    private val networkMonitor: NetworkMonitor
) {

    fun observeTransactions(): Flow<List<TransactionEntity>> = transactionDao.observeAll()

    fun observePendingCount(): Flow<Int> = transactionDao.observePendingCount()

    suspend fun refreshFromServer(): Result<Unit> {
        val result = safeApiCall { api.getTransactions() }
        return result.map { list ->
            transactionDao.clearSynced()
            transactionDao.insertAll(list.map { it.toEntity() })
        }
    }

    /**
     * Patrón offline-first: intenta la API; si no hay red, persiste en Room con
     * isSynced = false y ajusta el saldo local de forma optimista.
     */
    suspend fun createTransaction(
        accountId: Int,
        categoryId: Int?,
        categoryName: String,
        amount: BigDecimal,
        type: Int,
        description: String
    ): Result<CreateTransactionResult> {
        val request = CreateTransactionRequest(
            accountId = accountId,
            categoryId = categoryId,
            amount = amount,
            type = type,
            date = null,
            description = description
        )

        if (networkMonitor.isOnline()) {
            val result = safeApiCall { api.createTransaction(request) }
            result.onSuccess { dto ->
                transactionDao.insert(dto.toEntity())
                applyLocalBalanceImpact(accountId, amount, type)
                return Result.success(CreateTransactionResult(savedOffline = false))
            }
            val error = result.exceptionOrNull()
            if (error !is OfflineException) {
                return Result.failure(error ?: Exception("Error desconocido."))
            }
        }

        saveOffline(request, categoryName)
        return Result.success(CreateTransactionResult(savedOffline = true))
    }

    private suspend fun saveOffline(request: CreateTransactionRequest, categoryName: String) {
        val account = accountDao.getAll().firstOrNull { it.id == request.accountId }
        transactionDao.insert(
            TransactionEntity(
                serverId = null,
                accountId = request.accountId,
                accountName = account?.name ?: "Cuenta",
                currencyCode = account?.currencyCode ?: "COP",
                categoryId = request.categoryId,
                categoryName = categoryName.ifEmpty { "Sin categoría" },
                categoryIcon = "category",
                amount = request.amount,
                type = request.type,
                date = System.currentTimeMillis(),
                description = request.description,
                isSynced = false,
                createdAt = System.currentTimeMillis()
            )
        )
        applyLocalBalanceImpact(request.accountId, request.amount, request.type)
    }

    private suspend fun applyLocalBalanceImpact(accountId: Int, amount: BigDecimal, type: Int) {
        val account = accountDao.getAll().firstOrNull { it.id == accountId } ?: return
        val newBalance = if (type == 1) account.currentBalance.add(amount) else account.currentBalance.subtract(amount)
        accountDao.updateBalance(accountId, newBalance)
    }

    suspend fun suggestCategory(description: String): Result<CategorySuggestionDto> {
        return safeApiCall { api.suggestCategory(description) }
    }

    suspend fun getLocalMonthlySummary(): Pair<BigDecimal, BigDecimal> {
        val monthStart = DateUtils.startOfCurrentMonthMillis()
        val transactions = transactionDao.getSince(monthStart)
        var income = BigDecimal.ZERO
        var expense = BigDecimal.ZERO
        transactions.forEach {
            if (it.type == 1) income = income.add(it.amount) else expense = expense.add(it.amount)
        }
        return income to expense
    }
}
