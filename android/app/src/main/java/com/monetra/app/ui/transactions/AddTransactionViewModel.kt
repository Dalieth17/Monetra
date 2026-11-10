package com.monetra.app.ui.transactions

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelProvider.AndroidViewModelFactory.Companion.APPLICATION_KEY
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.monetra.app.MonetraApp
import com.monetra.app.data.local.entities.AccountEntity
import com.monetra.app.data.remote.dto.CategorySuggestionDto
import com.monetra.app.data.repository.DashboardRepository
import com.monetra.app.data.repository.TransactionRepository
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import java.math.BigDecimal

data class AddTransactionUiState(
    val accounts: List<AccountEntity> = emptyList(),
    val selectedAccount: AccountEntity? = null,
    val amountText: String = "",
    val description: String = "",
    val type: Int = 2, // 2 = Egreso por defecto (caso más frecuente)
    val suggestion: CategorySuggestionDto? = null,
    val acceptedCategoryId: Int? = null,
    val acceptedCategoryName: String = "",
    val isSaving: Boolean = false,
    val errorMessage: String? = null,
    val savedOffline: Boolean? = null
) {
    val amount: BigDecimal? get() = amountText.toBigDecimalOrNull()
    val isAmountValid: Boolean get() = (amount ?: BigDecimal.ZERO) > BigDecimal.ZERO
    val canSave: Boolean get() = selectedAccount != null && isAmountValid && !isSaving
}

private fun String.toBigDecimalOrNull(): BigDecimal? = try {
    if (isBlank()) null else BigDecimal(replace(",", "."))
} catch (_: NumberFormatException) {
    null
}

class AddTransactionViewModel(
    private val transactionRepository: TransactionRepository,
    private val dashboardRepository: DashboardRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(AddTransactionUiState())
    val uiState: StateFlow<AddTransactionUiState> = _uiState.asStateFlow()

    private var suggestionJob: Job? = null

    init {
        viewModelScope.launch {
            dashboardRepository.getDashboard("COP").onSuccess { data ->
                _uiState.update {
                    it.copy(accounts = data.accounts, selectedAccount = data.accounts.firstOrNull())
                }
            }
        }
    }

    fun onAccountSelected(account: AccountEntity) = _uiState.update { it.copy(selectedAccount = account) }

    fun onTypeChange(type: Int) = _uiState.update { it.copy(type = type) }

    fun onAmountChange(value: String) = _uiState.update { it.copy(amountText = value, errorMessage = null) }

    fun onDescriptionChange(value: String) {
        _uiState.update {
            it.copy(description = value, suggestion = null, acceptedCategoryId = null, acceptedCategoryName = "")
        }
        suggestionJob?.cancel()
        if (value.length < 4) return
        suggestionJob = viewModelScope.launch {
            delay(500) // debounce para no saturar el endpoint de IA
            transactionRepository.suggestCategory(value).onSuccess { suggestion ->
                _uiState.update { it.copy(suggestion = suggestion) }
            }
        }
    }

    fun acceptSuggestion() {
        val suggestion = _uiState.value.suggestion ?: return
        _uiState.update {
            it.copy(
                acceptedCategoryId = suggestion.categoryId,
                acceptedCategoryName = suggestion.categoryName
            )
        }
    }

    fun save() {
        val state = _uiState.value
        val account = state.selectedAccount ?: return
        val amount = state.amount ?: return
        _uiState.update { it.copy(isSaving = true, errorMessage = null) }

        viewModelScope.launch {
            transactionRepository.createTransaction(
                accountId = account.id,
                categoryId = state.acceptedCategoryId,
                categoryName = state.acceptedCategoryName,
                amount = amount,
                type = state.type,
                description = state.description.trim()
            )
                .onSuccess { result ->
                    _uiState.update { it.copy(isSaving = false, savedOffline = result.savedOffline) }
                }
                .onFailure { error ->
                    _uiState.update { it.copy(isSaving = false, errorMessage = error.message) }
                }
        }
    }

    companion object {
        val Factory: ViewModelProvider.Factory = viewModelFactory {
            initializer {
                val app = this[APPLICATION_KEY] as MonetraApp
                AddTransactionViewModel(
                    app.container.transactionRepository,
                    app.container.dashboardRepository
                )
            }
        }
    }
}
