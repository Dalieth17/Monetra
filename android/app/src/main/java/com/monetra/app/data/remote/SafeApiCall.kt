package com.monetra.app.data.remote

import com.google.gson.Gson
import com.monetra.app.data.remote.dto.ApiResponse
import retrofit2.HttpException
import java.io.IOException

class OfflineException(message: String = "Sin conexión con el servidor.") : IOException(message)

class ApiErrorException(message: String, val errors: Map<String, List<String>>? = null) : Exception(message)

suspend fun <T> safeApiCall(call: suspend () -> ApiResponse<T>): Result<T> {
    return try {
        val response = call()
        if (response.success && response.data != null) {
            Result.success(response.data)
        } else {
            Result.failure(ApiErrorException(response.message.ifEmpty { "Operación fallida." }, response.errors))
        }
    } catch (e: HttpException) {
        val parsed = parseErrorBody(e)
        Result.failure(parsed)
    } catch (e: IOException) {
        Result.failure(OfflineException())
    }
}

private fun parseErrorBody(e: HttpException): Exception {
    return try {
        val body = e.response()?.errorBody()?.string()
        val apiResponse = Gson().fromJson(body, ApiResponse::class.java)
        val message = apiResponse?.message?.ifEmpty { null }
            ?: apiResponse?.errors?.values?.flatten()?.firstOrNull()
            ?: defaultHttpMessage(e.code())
        @Suppress("UNCHECKED_CAST")
        ApiErrorException(message, apiResponse?.errors as? Map<String, List<String>>)
    } catch (_: Exception) {
        ApiErrorException(defaultHttpMessage(e.code()))
    }
}

private fun defaultHttpMessage(code: Int): String = when (code) {
    401 -> "Sesión expirada. Inicia sesión nuevamente."
    403 -> "No tienes permisos para esta operación."
    404 -> "Recurso no encontrado."
    else -> "Error del servidor ($code). Intenta de nuevo."
}
