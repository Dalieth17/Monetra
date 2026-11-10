package com.monetra.app.data.remote.dto

import java.math.BigDecimal

data class MarketRateDto(
    val baseCurrency: String,
    val targetCurrency: String,
    val rate: BigDecimal,
    val lastUpdated: String,
    val isStale: Boolean,
    val warningMessage: String
)

data class CategorySuggestionDto(
    val categoryId: Int?,
    val categoryName: String,
    val confidence: Double,
    val reason: String
)
