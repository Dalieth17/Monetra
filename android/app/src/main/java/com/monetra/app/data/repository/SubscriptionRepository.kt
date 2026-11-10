package com.monetra.app.data.repository

import com.monetra.app.data.local.dao.SubscriptionDao
import com.monetra.app.data.local.entities.SubscriptionEntity
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.dto.CreateSubscriptionRequest
import com.monetra.app.data.remote.dto.SubscriptionDto
import com.monetra.app.data.remote.safeApiCall
import kotlinx.coroutines.flow.Flow
import java.math.BigDecimal

class SubscriptionRepository(
    private val api: MonetraApi,
    private val subscriptionDao: SubscriptionDao
) {

    fun observeSubscriptions(): Flow<List<SubscriptionEntity>> = subscriptionDao.observeAll()

    suspend fun refresh(): Result<Unit> {
        return safeApiCall { api.getSubscriptions() }.map { list ->
            subscriptionDao.clearAll()
            subscriptionDao.upsertAll(list.map { it.toEntity() })
        }
    }

    suspend fun createSubscription(
        name: String,
        amount: BigDecimal,
        currencyCode: String,
        billingCycle: String,
        nextPaymentDateIso: String
    ): Result<SubscriptionDto> {
        return safeApiCall {
            api.createSubscription(CreateSubscriptionRequest(name, amount, currencyCode, billingCycle, nextPaymentDateIso))
        }.onSuccess { refresh() }
    }

    suspend fun toggle(subscriptionId: Int): Result<SubscriptionDto> {
        return safeApiCall { api.toggleSubscription(subscriptionId) }.onSuccess { refresh() }
    }

    suspend fun delete(subscriptionId: Int): Result<Boolean> {
        return safeApiCall { api.deleteSubscription(subscriptionId) }.onSuccess { refresh() }
    }
}
