package com.monetra.app.ui.savings

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelProvider.AndroidViewModelFactory.Companion.APPLICATION_KEY
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.monetra.app.MonetraApp
import com.monetra.app.core.DateUtils
import com.monetra.app.data.local.entities.AccountEntity
import com.monetra.app.data.local.entities.SavingsGoalEntity
import com.monetra.app.data.repository.DashboardRepository
import com.monetra.app.data.repository.SavingsGoalRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import java.math.BigDecimal

data class SavingsUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val accounts: List<AccountEntity> = emptyList(),
    val completedGoalName: String? = null
)

class SavingsGoalsViewModel(
    private val savingsGoalRepository: SavingsGoalRepository,
    private val dashboardRepository: DashboardRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(SavingsUiState())
    val uiState: StateFlow<SavingsUiState> = _uiState.asStateFlow()

    val goals: StateFlow<List<SavingsGoalEntity>> = savingsGoalRepository.observeGoals()
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
            savingsGoalRepository.refresh()
                .onSuccess { _uiState.update { it.copy(isLoading = false) } }
                .onFailure { error ->
                    _uiState.update { it.copy(isLoading = false, errorMessage = error.message) }
                }
        }
    }

    fun createGoal(name: String, targetAmount: BigDecimal, deadlineMillis: Long) {
        viewModelScope.launch {
            savingsGoalRepository.createGoal(name, targetAmount, DateUtils.millisToIso(deadlineMillis))
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun deposit(goalId: Int, accountId: Int, amount: BigDecimal) {
        viewModelScope.launch {
            savingsGoalRepository.deposit(goalId, accountId, amount)
                .onSuccess { goal ->
                    if (goal.isCompleted) {
                        _uiState.update { it.copy(completedGoalName = goal.name) }
                    }
                }
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun deleteGoal(goalId: Int) {
        viewModelScope.launch {
            savingsGoalRepository.deleteGoal(goalId)
                .onFailure { error -> _uiState.update { it.copy(errorMessage = error.message) } }
        }
    }

    fun dismissCelebration() = _uiState.update { it.copy(completedGoalName = null) }
    fun dismissError() = _uiState.update { it.copy(errorMessage = null) }

    companion object {
        val Factory: ViewModelProvider.Factory = viewModelFactory {
            initializer {
                val app = this[APPLICATION_KEY] as MonetraApp
                SavingsGoalsViewModel(app.container.savingsGoalRepository, app.container.dashboardRepository)
            }
        }
    }
}
