package com.monetra.app.data.remote.dto

import java.math.BigDecimal

data class DebtDto(
    val id: Int,
    val creditor: String,
    val totalAmount: BigDecimal,
    val remainingAmount: BigDecimal,
    val interestRate: BigDecimal,
    val rateType: String,
    val dueDate: String,
    val estimatedMonthlyPayment: BigDecimal,
    val createdAt: String
)

data class CreateDebtRequest(
    val creditor: String,
    val totalAmount: BigDecimal,
    val interestRate: BigDecimal,
    val rateType: String,
    val dueDate: String
)

data class RecordDebtPaymentRequest(
    val accountId: Int,
    val amount: BigDecimal
)
