package com.monetra.app.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import java.math.BigDecimal

@Entity(tableName = "debt_table")
data class DebtEntity(
    @PrimaryKey val id: Int,
    val creditor: String,
    val totalAmount: BigDecimal,
    val remainingAmount: BigDecimal,
    val interestRate: BigDecimal,
    val rateType: String,
    val dueDate: Long,
    val estimatedMonthlyPayment: BigDecimal,
    val createdAt: Long
)
