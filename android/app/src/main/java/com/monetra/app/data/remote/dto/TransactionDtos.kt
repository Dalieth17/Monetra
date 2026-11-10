package com.monetra.app.data.remote.dto

import java.math.BigDecimal

data class TransactionDto(
    val id: Int,
    val accountId: Int,
    val accountName: String,
    val currencyCode: String,
    val categoryId: Int?,
    val categoryName: String,
    val categoryIcon: String,
    val amount: BigDecimal,
    val type: Int,
    val date: String,
    val description: String,
    val createdAt: String
)

data class CreateTransactionRequest(
    val accountId: Int,
    val categoryId: Int?,
    val amount: BigDecimal,
    val type: Int,
    val date: String?,
    val description: String
)
