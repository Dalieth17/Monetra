package com.monetra.app.data.repository

import com.monetra.app.data.local.dao.DebtDao
import com.monetra.app.data.local.entities.DebtEntity
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.dto.CreateDebtRequest
import com.monetra.app.data.remote.dto.DebtDto
import com.monetra.app.data.remote.dto.RecordDebtPaymentRequest
import com.monetra.app.data.remote.safeApiCall
import kotlinx.coroutines.flow.Flow
import java.math.BigDecimal

class DebtRepository(
    private val api: MonetraApi,
    private val debtDao: DebtDao
) {

    fun observeDebts(): Flow<List<DebtEntity>> = debtDao.observeAll()

    suspend fun refresh(): Result<Unit> {
        return safeApiCall { api.getDebts() }.map { list ->
            debtDao.clearAll()
            debtDao.upsertAll(list.map { it.toEntity() })
        }
    }

    suspend fun createDebt(creditor: String, totalAmount: BigDecimal, interestRate: BigDecimal, rateType: String, dueDateIso: String): Result<DebtDto> {
        return safeApiCall {
            api.createDebt(CreateDebtRequest(creditor, totalAmount, interestRate, rateType, dueDateIso))
        }.onSuccess { refresh() }
    }

    suspend fun recordPayment(debtId: Int, accountId: Int, amount: BigDecimal): Result<DebtDto> {
        return safeApiCall {
            api.recordDebtPayment(debtId, RecordDebtPaymentRequest(accountId, amount))
        }.onSuccess { refresh() }
    }

    suspend fun deleteDebt(debtId: Int): Result<Boolean> {
        return safeApiCall { api.deleteDebt(debtId) }.onSuccess { refresh() }
    }
}
