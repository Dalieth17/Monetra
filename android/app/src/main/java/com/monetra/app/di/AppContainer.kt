package com.monetra.app.di

import android.content.Context
import com.monetra.app.core.Constants
import com.monetra.app.core.NetworkMonitor
import com.monetra.app.data.local.MonetraDatabase
import com.monetra.app.data.local.SessionManager
import com.monetra.app.data.remote.AuthInterceptor
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.repository.AuthRepository
import com.monetra.app.data.repository.DashboardRepository
import com.monetra.app.data.repository.DebtRepository
import com.monetra.app.data.repository.SavingsGoalRepository
import com.monetra.app.data.repository.SubscriptionRepository
import com.monetra.app.data.repository.TransactionRepository
import com.monetra.app.data.sync.SyncManager
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

/** Contenedor de inyección de dependencias manual (sin frameworks adicionales). */
class AppContainer(context: Context) {

    val sessionManager = SessionManager(context)
    val networkMonitor = NetworkMonitor(context)

    private val database = MonetraDatabase.getInstance(context)

    private val okHttpClient = OkHttpClient.Builder()
        .addInterceptor(AuthInterceptor(sessionManager))
        .addInterceptor(HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BASIC })
        .connectTimeout(10, TimeUnit.SECONDS)
        .readTimeout(20, TimeUnit.SECONDS)
        .build()

    private val api: MonetraApi = Retrofit.Builder()
        .baseUrl(Constants.BASE_URL)
        .client(okHttpClient)
        .addConverterFactory(GsonConverterFactory.create())
        .build()
        .create(MonetraApi::class.java)

    val authRepository = AuthRepository(api, sessionManager)

    val transactionRepository = TransactionRepository(
        api, database.transactionDao(), database.accountDao(), networkMonitor
    )

    val dashboardRepository = DashboardRepository(
        api, database.accountDao(), transactionRepository, sessionManager
    )

    val debtRepository = DebtRepository(api, database.debtDao())
    val savingsGoalRepository = SavingsGoalRepository(api, database.savingsGoalDao())
    val subscriptionRepository = SubscriptionRepository(api, database.subscriptionDao())

    val syncManager = SyncManager(api, database.transactionDao(), networkMonitor)
}
