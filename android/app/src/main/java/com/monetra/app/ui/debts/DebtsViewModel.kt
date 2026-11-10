package com.monetra.app.ui.debts

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelProvider.AndroidViewModelFactory.Companion.APPLICATION_KEY
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.monetra.app.MonetraApp
import com.monetra.app.core.DateUtils
import com.monetra.app.data.local.entities.AccountEntity
import com.monetra.app.data.local.entities.DebtEntity
import com.monetra.app.data.repository.DashboardRepository
import com.monetra.app.data.repository.DebtRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import java.math.BigDecimal

data class DebtsUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val infoMessage: String? = null,
    val accounts: List<AccountEntity> = emptyList()
)

class DebtsViewModel(
    private val debtRepository: DebtRepository,
    private val dashboardRepository: DashboardRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(DebtsUiState())
    val uiState: StateFlow<DebtsUiState> = _uiState.asStateFlow()

    val debts: StateFlow<List<DebtEntity>> = debtRepository.observeDebts()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), emptyList())

    init {
        refresh()
        viewModelScope.launch {
            dashboardRepository.getDashboard("COP").onSuccess { data ->
                _uiState.update { it.copy(accounts = data.accounts) }
            }
        }
    }

    fun refresh() {
        _uiState.update { it.copy(isLoading = true, errorMessage = null) }
        viewModelScope.launch {
            debtRepository.refresh()
                .onSuccess { _uiState.update { it.copy(isLoading = false) } }
                .onFailure { error ->
                    _uiState.update { it.copy(isLoading = false, errorMessage = error.message) }
                }
        }
    }

    fun createDebt(creditor: String, totalAmount: BigDecimal, interestRate: BigDecimal, rateType: String, dueDateMillis: Long) {
        viewModelScope.launch {
            debtRepository.createDebt(creditor, totalAmount, interestRate, rateType, DateUtils.millisToIso(dueDateMillis))
                .onSuccess { _uiState.update { it.copy(infoMessage = "Deuda registrada exitosamente.") } }
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun recordPayment(debtId: Int, accountId: Int, amount: BigDecimal) {
        viewModelScope.launch {
            debtRepository.recordPayment(debtId, accountId, amount)
                .onSuccess { _uiState.update { it.copy(infoMessage = "Abono aplicado a la deuda.") } }
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun deleteDebt(debtId: Int) {
        viewModelScope.launch {
            debtRepository.deleteDebt(debtId)
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun dismissMessages() = _uiState.update { it.copy(infoMessage = null, errorMessage = null) }

    companion object {
        val Factory: ViewModelProvider.Factory = viewModelFactory {
            initializer {
                val app = this[APPLICATION_KEY] as MonetraApp
                DebtsViewModel(app.container.debtRepository, app.container.dashboardRepository)
            }
        }
    }
}
