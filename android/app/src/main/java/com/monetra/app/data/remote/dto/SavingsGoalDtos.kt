package com.monetra.app.data.remote.dto

import java.math.BigDecimal

data class SavingsGoalDto(
    val id: Int,
    val name: String,
    val targetAmount: BigDecimal,
    val currentAmount: BigDecimal,
    val progressPercentage: Double,
    val deadline: String,
    val isCompleted: Boolean,
    val createdAt: String
)

data class CreateSavingsGoalRequest(
    val name: String,
    val targetAmount: BigDecimal,
    val deadline: String
)

data class DepositSavingsGoalRequest(
    val accountId: Int,
    val amount: BigDecimal
)
