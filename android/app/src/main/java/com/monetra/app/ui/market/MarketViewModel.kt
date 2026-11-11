package com.monetra.app.ui.market

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelProvider.AndroidViewModelFactory.Companion.APPLICATION_KEY
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.monetra.app.MonetraApp
import com.monetra.app.data.remote.dto.MarketRateDto
import com.monetra.app.data.remote.dto.StockQuoteDto
import com.monetra.app.data.repository.MarketRepository
import kotlinx.coroutines.async
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class MarketUiState(
    val isLoading: Boolean = true,
    val errorMessage: String? = null,
    val rate: MarketRateDto? = null,
    val stocks: List<StockQuoteDto> = emptyList(),
    val expandedSymbol: String? = null
)

class MarketViewModel(private val marketRepository: MarketRepository) : ViewModel() {

    private val _uiState = MutableStateFlow(MarketUiState())
    val uiState: StateFlow<MarketUiState> = _uiState.asStateFlow()

    init {
        load()
    }

    fun load() {
        _uiState.update { it.copy(isLoading = true, errorMessage = null) }
        viewModelScope.launch {
            val rateDeferred = async { marketRepository.getUsdCopRate() }
            val stocksDeferred = async { marketRepository.getStocks() }
            val rateResult = rateDeferred.await()
            val stocksResult = stocksDeferred.await()

            rateResult
                .onSuccess { rate ->
                    _uiState.update {
                        it.copy(isLoading = false, rate = rate, stocks = stocksResult.getOrDefault(emptyList()))
                    }
                }
                .onFailure { error ->
                    _uiState.update { it.copy(isLoading = false, errorMessage = error.message) }
                }
        }
    }

    fun toggleExpanded(symbol: String) {
        _uiState.update { it.copy(expandedSymbol = if (it.expandedSymbol == symbol) null else symbol) }
    }

    companion object {
        val Factory: ViewModelProvider.Factory = viewModelFactory {
            initializer {
                val app = this[APPLICATION_KEY] as MonetraApp
                MarketViewModel(app.container.marketRepository)
            }
        }
    }
}
