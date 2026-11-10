package com.monetra.app.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import java.math.BigDecimal

@Entity(tableName = "account_table")
data class AccountEntity(
    @PrimaryKey val id: Int,
    val name: String,
    val currencyCode: String,
    val currentBalance: BigDecimal,
    val convertedBalance: BigDecimal,
    val createdAt: Long
)
