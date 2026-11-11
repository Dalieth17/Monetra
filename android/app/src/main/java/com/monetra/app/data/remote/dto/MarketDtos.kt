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

data class StockQuoteDto(
    val symbol: String,
    val name: String,
    val price: BigDecimal,
    val previousClose: BigDecimal,
    val change: BigDecimal,
    val changePercent: BigDecimal,
    val currency: String,
    val lastUpdated: String,
    val isStale: Boolean,
    val warningMessage: String,
    val history: List<StockPricePointDto>
)

data class StockPricePointDto(
    val date: String,
    val close: BigDecimal
)

data class CategorySuggestionDto(
    val categoryId: Int?,
    val categoryName: String,
    val confidence: Double,
    val reason: String
)
