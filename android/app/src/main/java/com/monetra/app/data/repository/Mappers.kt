package com.monetra.app.data.repository

import com.monetra.app.core.DateUtils
import com.monetra.app.data.local.entities.AccountEntity
import com.monetra.app.data.local.entities.DebtEntity
import com.monetra.app.data.local.entities.SavingsGoalEntity
import com.monetra.app.data.local.entities.SubscriptionEntity
import com.monetra.app.data.local.entities.TransactionEntity
import com.monetra.app.data.remote.dto.AccountDto
import com.monetra.app.data.remote.dto.DebtDto
import com.monetra.app.data.remote.dto.SavingsGoalDto
import com.monetra.app.data.remote.dto.SubscriptionDto
import com.monetra.app.data.remote.dto.TransactionDto

fun AccountDto.toEntity() = AccountEntity(
    id = id,
    name = name,
    currencyCode = currencyCode,
    currentBalance = currentBalance,
    convertedBalance = convertedBalance,
    createdAt = DateUtils.parseIsoToMillis(createdAt)
)

fun TransactionDto.toEntity() = TransactionEntity(
    serverId = id,
    accountId = accountId,
    accountName = accountName,
    currencyCode = currencyCode,
    categoryId = categoryId,
    categoryName = categoryName,
    categoryIcon = categoryIcon,
    amount = amount,
    type = type,
    date = DateUtils.parseIsoToMillis(date),
    description = description,
    isSynced = true,
    createdAt = DateUtils.parseIsoToMillis(createdAt)
)

fun DebtDto.toEntity() = DebtEntity(
    id = id,
    creditor = creditor,
    totalAmount = totalAmount,
    remainingAmount = remainingAmount,
    interestRate = interestRate,
    rateType = rateType,
    dueDate = DateUtils.parseIsoToMillis(dueDate),
    estimatedMonthlyPayment = estimatedMonthlyPayment,
    createdAt = DateUtils.parseIsoToMillis(createdAt)
)

fun SavingsGoalDto.toEntity() = SavingsGoalEntity(
    id = id,
    name = name,
    targetAmount = targetAmount,
    currentAmount = currentAmount,
    progressPercentage = progressPercentage,
    isCompleted = isCompleted,
    deadline = DateUtils.parseIsoToMillis(deadline),
    createdAt = DateUtils.parseIsoToMillis(createdAt)
)

fun SubscriptionDto.toEntity() = SubscriptionEntity(
    id = id,
    name = name,
    amount = amount,
    currencyCode = currencyCode,
    billingCycle = billingCycle,
    nextPaymentDate = DateUtils.parseIsoToMillis(nextPaymentDate),
    daysUntilPayment = daysUntilPayment,
    isActive = isActive,
    createdAt = DateUtils.parseIsoToMillis(createdAt)
)
