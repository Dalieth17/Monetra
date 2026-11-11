package com.monetra.app.data.repository

import com.monetra.app.data.local.SessionManager
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.dto.AuthResponse
import com.monetra.app.data.remote.dto.LoginRequest
import com.monetra.app.data.remote.dto.RegisterRequest
import com.monetra.app.data.remote.safeApiCall

class AuthRepository(
    private val api: MonetraApi,
    private val sessionManager: SessionManager,
    private val onEnterDemo: () -> Unit = {}
) {

    /** Activa el modo demostración con datos de ejemplo frescos, sin red. */
    fun enterDemo() {
        onEnterDemo()
        sessionManager.enterDemo()
    }

    suspend fun login(email: String, password: String): Result<AuthResponse> {
        return safeApiCall { api.login(LoginRequest(email.trim(), password)) }
            .onSuccess { saveSession(it) }
    }

    suspend fun register(name: String, email: String, password: String): Result<AuthResponse> {
        return safeApiCall { api.register(RegisterRequest(name.trim(), email.trim(), password)) }
            .onSuccess { saveSession(it) }
    }

    fun isLoggedIn(): Boolean = sessionManager.isLoggedIn()

    fun logout() = sessionManager.clear()

    private fun saveSession(auth: AuthResponse) {
        sessionManager.saveSession(auth.token, auth.userId, auth.name, auth.email, auth.role)
    }
}
