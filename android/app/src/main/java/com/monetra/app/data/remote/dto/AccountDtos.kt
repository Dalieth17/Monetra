package com.monetra.app.data.remote.dto

import java.math.BigDecimal

data class AccountDto(
    val id: Int,
    val name: String,
    val currencyCode: String,
    val currentBalance: BigDecimal,
    val convertedBalance: BigDecimal,
    val createdAt: String
)

data class CreateAccountRequest(
    val name: String,
    val currencyCode: String,
    val initialBalance: BigDecimal
)
