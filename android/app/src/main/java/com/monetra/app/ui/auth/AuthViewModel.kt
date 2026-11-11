package com.monetra.app.ui.auth

import android.util.Patterns
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelProvider.AndroidViewModelFactory.Companion.APPLICATION_KEY
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.monetra.app.MonetraApp
import com.monetra.app.data.repository.AuthRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class AuthUiState(
    val name: String = "",
    val email: String = "",
    val password: String = "",
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val isAuthenticated: Boolean = false
) {
    val isEmailValid: Boolean get() = Patterns.EMAIL_ADDRESS.matcher(email).matches()
    val isPasswordValid: Boolean get() = password.length >= 6
    val canLogin: Boolean get() = isEmailValid && password.isNotEmpty() && !isLoading
    val canRegister: Boolean get() = name.isNotBlank() && isEmailValid && isPasswordValid && !isLoading
}

class AuthViewModel(private val authRepository: AuthRepository) : ViewModel() {

    private val _uiState = MutableStateFlow(AuthUiState())
    val uiState: StateFlow<AuthUiState> = _uiState.asStateFlow()

    fun onNameChange(value: String) = _uiState.update { it.copy(name = value, errorMessage = null) }
    fun onEmailChange(value: String) = _uiState.update { it.copy(email = value, errorMessage = null) }
    fun onPasswordChange(value: String) = _uiState.update { it.copy(password = value, errorMessage = null) }

    fun login() {
        val state = _uiState.value
        if (!state.canLogin) return
        _uiState.update { it.copy(isLoading = true, errorMessage = null) }
        viewModelScope.launch {
            authRepository.login(state.email, state.password)
                .onSuccess { _uiState.update { it.copy(isLoading = false, isAuthenticated = true) } }
                .onFailure { error ->
                    _uiState.update { it.copy(isLoading = false, errorMessage = error.message) }
                }
        }
    }

    fun enterDemo() {
        authRepository.enterDemo()
        _uiState.update { it.copy(isAuthenticated = true) }
    }

    fun register() {
        val state = _uiState.value
        if (!state.canRegister) return
        _uiState.update { it.copy(isLoading = true, errorMessage = null) }
        viewModelScope.launch {
            authRepository.register(state.name, state.email, state.password)
                .onSuccess { _uiState.update { it.copy(isLoading = false, isAuthenticated = true) } }
                .onFailure { error ->
                    _uiState.update { it.copy(isLoading = false, errorMessage = error.message) }
                }
        }
    }

    companion object {
        val Factory: ViewModelProvider.Factory = viewModelFactory {
            initializer {
                val app = this[APPLICATION_KEY] as MonetraApp
                AuthViewModel(app.container.authRepository)
            }
        }
    }
}
