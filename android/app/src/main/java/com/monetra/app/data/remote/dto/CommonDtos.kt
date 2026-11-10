package com.monetra.app.data.remote.dto

data class ApiResponse<T>(
    val success: Boolean,
    val message: String = "",
    val data: T?,
    val errors: Map<String, List<String>>? = null
)
