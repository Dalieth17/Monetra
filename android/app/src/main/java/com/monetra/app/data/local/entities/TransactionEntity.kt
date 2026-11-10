package com.monetra.app.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import java.math.BigDecimal

@Entity(tableName = "transaction_table")
data class TransactionEntity(
    @PrimaryKey(autoGenerate = true) val localId: Long = 0,
    val serverId: Int?,
    val accountId: Int,
    val accountName: String,
    val currencyCode: String,
    val categoryId: Int?,
    val categoryName: String,
    val categoryIcon: String,
    val amount: BigDecimal,
    val type: Int, // 1 = Income, 2 = Expense
    val date: Long,
    val description: String,
    val isSynced: Boolean,
    val createdAt: Long
)
