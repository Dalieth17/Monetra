package com.monetra.app.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import java.math.BigDecimal

@Entity(tableName = "subscription_table")
data class SubscriptionEntity(
    @PrimaryKey val id: Int,
    val name: String,
    val amount: BigDecimal,
    val currencyCode: String,
    val billingCycle: String,
    val nextPaymentDate: Long,
    val daysUntilPayment: Int,
    val isActive: Boolean,
    val createdAt: Long
)
