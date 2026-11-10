package com.monetra.app.data.remote.dto

data class RegisterRequest(
    val name: String,
    val email: String,
    val password: String,
    val role: String? = null
)

data class LoginRequest(
    val email: String,
    val password: String
)

data class AuthResponse(
    val token: String,
    val userId: Int,
    val name: String,
    val email: String,
    val role: String
)
