package com.monetra.app.data.repository

import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.dto.MarketRateDto
import com.monetra.app.data.remote.dto.StockQuoteDto
import com.monetra.app.data.remote.safeApiCall

class MarketRepository(private val api: MonetraApi) {

    suspend fun getUsdCopRate(): Result<MarketRateDto> = safeApiCall { api.getUsdCopRate() }

    suspend fun getStocks(): Result<List<StockQuoteDto>> = safeApiCall { api.getStocks() }
}
