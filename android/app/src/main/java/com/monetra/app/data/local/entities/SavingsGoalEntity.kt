package com.monetra.app.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import java.math.BigDecimal

@Entity(tableName = "savings_goal_table")
data class SavingsGoalEntity(
    @PrimaryKey val id: Int,
    val name: String,
    val targetAmount: BigDecimal,
    val currentAmount: BigDecimal,
    val progressPercentage: Double,
    val isCompleted: Boolean,
    val deadline: Long,
    val createdAt: Long
)
