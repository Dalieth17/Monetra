package com.monetra.app.data.remote.dto

import java.math.BigDecimal

data class SubscriptionDto(
    val id: Int,
    val name: String,
    val amount: BigDecimal,
    val currencyCode: String,
    val billingCycle: String,
    val nextPaymentDate: String,
    val isActive: Boolean,
    val daysUntilPayment: Int,
    val createdAt: String
)

data class CreateSubscriptionRequest(
    val name: String,
    val amount: BigDecimal,
    val currencyCode: String,
    val billingCycle: String,
    val nextPaymentDate: String
)
