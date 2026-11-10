package com.monetra.app.ui.dashboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelProvider.AndroidViewModelFactory.Companion.APPLICATION_KEY
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.monetra.app.MonetraApp
import com.monetra.app.data.repository.AuthRepository
import com.monetra.app.data.repository.DashboardData
import com.monetra.app.data.repository.DashboardRepository
import com.monetra.app.data.repository.TransactionRepository
import com.monetra.app.data.sync.SyncManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class DashboardUiState(
    val isLoading: Boolean = true,
    val errorMessage: String? = null,
    val baseCurrency: String = "COP",
    val data: DashboardData? = null,
    val syncedMessage: String? = null
)

class DashboardViewModel(
    private val dashboardRepository: DashboardRepository,
    private val transactionRepository: TransactionRepository,
    private val authRepository: AuthRepository,
    private val syncManager: SyncManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(DashboardUiState())
    val uiState: StateFlow<DashboardUiState> = _uiState.asStateFlow()

    val pendingSyncCount: StateFlow<Int> = transactionRepository.observePendingCount()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), 0)

    init {
        load()
    }

    fun load() {
        _uiState.update { it.copy(isLoading = true, errorMessage = null, syncedMessage = null) }
        viewModelScope.launch {
            val synced = syncManager.syncPendingTransactions()

            dashboardRepository.getDashboard(_uiState.value.baseCurrency)
                .onSuccess { data ->
                    _uiState.update {
                        it.copy(
                            isLoading = false,
                            data = data,
                            syncedMessage = if (synced > 0) "$synced movimiento(s) sincronizado(s) con el servidor." else null
                        )
                    }
                }
                .onFailure { error ->
                    _uiState.update { it.copy(isLoading = false, errorMessage = error.message) }
                }
        }
    }

    fun toggleBaseCurrency() {
        val newCurrency = if (_uiState.value.baseCurrency == "COP") "USD" else "COP"
        _uiState.update { it.copy(baseCurrency = newCurrency) }
        load()
    }

    fun dismissSyncMessage() = _uiState.update { it.copy(syncedMessage = null) }

    fun logout() = authRepository.logout()

    companion object {
        val Factory: ViewModelProvider.Factory = viewModelFactory {
            initializer {
                val app = this[APPLICATION_KEY] as MonetraApp
                DashboardViewModel(
                    app.container.dashboardRepository,
                    app.container.transactionRepository,
                    app.container.authRepository,
                    app.container.syncManager
                )
            }
        }
    }
}
