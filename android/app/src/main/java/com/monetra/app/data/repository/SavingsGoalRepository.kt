package com.monetra.app.data.repository

import com.monetra.app.data.local.dao.SavingsGoalDao
import com.monetra.app.data.local.entities.SavingsGoalEntity
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.dto.CreateSavingsGoalRequest
import com.monetra.app.data.remote.dto.DepositSavingsGoalRequest
import com.monetra.app.data.remote.dto.SavingsGoalDto
import com.monetra.app.data.remote.safeApiCall
import kotlinx.coroutines.flow.Flow
import java.math.BigDecimal

class SavingsGoalRepository(
    private val api: MonetraApi,
    private val savingsGoalDao: SavingsGoalDao
) {

    fun observeGoals(): Flow<List<SavingsGoalEntity>> = savingsGoalDao.observeAll()

    suspend fun refresh(): Result<Unit> {
        return safeApiCall { api.getSavingsGoals() }.map { list ->
            savingsGoalDao.clearAll()
            savingsGoalDao.upsertAll(list.map { it.toEntity() })
        }
    }

    suspend fun createGoal(name: String, targetAmount: BigDecimal, deadlineIso: String): Result<SavingsGoalDto> {
        return safeApiCall {
            api.createSavingsGoal(CreateSavingsGoalRequest(name, targetAmount, deadlineIso))
        }.onSuccess { refresh() }
    }

    suspend fun deposit(goalId: Int, accountId: Int, amount: BigDecimal): Result<SavingsGoalDto> {
        return safeApiCall {
            api.depositToGoal(goalId, DepositSavingsGoalRequest(accountId, amount))
        }.onSuccess { refresh() }
    }

    suspend fun deleteGoal(goalId: Int): Result<Boolean> {
        return safeApiCall { api.deleteSavingsGoal(goalId) }.onSuccess { refresh() }
    }
}
