package com.monetra.app.ui.subscriptions

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelProvider.AndroidViewModelFactory.Companion.APPLICATION_KEY
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.monetra.app.MonetraApp
import com.monetra.app.core.DateUtils
import com.monetra.app.data.local.entities.SubscriptionEntity
import com.monetra.app.data.repository.SubscriptionRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import java.math.BigDecimal

data class SubscriptionsUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null
)

class SubscriptionsViewModel(
    private val subscriptionRepository: SubscriptionRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(SubscriptionsUiState())
    val uiState: StateFlow<SubscriptionsUiState> = _uiState.asStateFlow()

    val subscriptions: StateFlow<List<SubscriptionEntity>> = subscriptionRepository.observeSubscriptions()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), emptyList())

    init {
        refresh()
    }

    fun refresh() {
        _uiState.update { it.copy(isLoading = true, errorMessage = null) }
        viewModelScope.launch {
            subscriptionRepository.refresh()
                .onSuccess { _uiState.update { it.copy(isLoading = false) } }
                .onFailure { error ->
                    _uiState.update { it.copy(isLoading = false, errorMessage = error.message) }
                }
        }
    }

    fun create(name: String, amount: BigDecimal, currencyCode: String, billingCycle: String, nextPaymentMillis: Long) {
        viewModelScope.launch {
            subscriptionRepository.createSubscription(
                name, amount, currencyCode, billingCycle, DateUtils.millisToIso(nextPaymentMillis)
            ).onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun toggle(subscriptionId: Int) {
        viewModelScope.launch {
            subscriptionRepository.toggle(subscriptionId)
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun delete(subscriptionId: Int) {
        viewModelScope.launch {
            subscriptionRepository.delete(subscriptionId)
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun dismissError() = _uiState.update { it.copy(errorMessage = null) }

    companion object {
        val Factory: ViewModelProvider.Factory = viewModelFactory {
            initializer {
                val app = this[APPLICATION_KEY] as MonetraApp
                SubscriptionsViewModel(app.container.subscriptionRepository)
            }
        }
    }
}
