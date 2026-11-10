package com.monetra.app.data.local

import android.content.Context
import androidx.core.content.edit
import java.math.BigDecimal

class SessionManager(context: Context) {

    private val prefs = context.getSharedPreferences("monetra_session", Context.MODE_PRIVATE)

    var token: String?
        get() = prefs.getString(KEY_TOKEN, null)
        private set(value) = prefs.edit { putString(KEY_TOKEN, value) }

    val userId: Int get() = prefs.getInt(KEY_USER_ID, -1)
    val userName: String get() = prefs.getString(KEY_NAME, "") ?: ""
    val userEmail: String get() = prefs.getString(KEY_EMAIL, "") ?: ""

    fun isLoggedIn(): Boolean = !token.isNullOrEmpty()

    fun saveSession(token: String, userId: Int, name: String, email: String, role: String) {
        prefs.edit {
            putString(KEY_TOKEN, token)
            putInt(KEY_USER_ID, userId)
            putString(KEY_NAME, name)
            putString(KEY_EMAIL, email)
            putString(KEY_ROLE, role)
        }
    }

    fun saveLastExchangeRate(rate: BigDecimal, timestampMillis: Long) {
        prefs.edit {
            putString(KEY_LAST_RATE, rate.toPlainString())
            putLong(KEY_LAST_RATE_AT, timestampMillis)
        }
    }

    fun lastExchangeRate(): BigDecimal? = prefs.getString(KEY_LAST_RATE, null)?.let { BigDecimal(it) }

    fun lastExchangeRateAt(): Long = prefs.getLong(KEY_LAST_RATE_AT, 0L)

    fun clear() {
        prefs.edit { clear() }
    }

    private companion object {
        const val KEY_TOKEN = "jwt_token"
        const val KEY_USER_ID = "user_id"
        const val KEY_NAME = "user_name"
        const val KEY_EMAIL = "user_email"
        const val KEY_ROLE = "user_role"
        const val KEY_LAST_RATE = "last_usd_cop_rate"
        const val KEY_LAST_RATE_AT = "last_usd_cop_rate_at"
    }
}
