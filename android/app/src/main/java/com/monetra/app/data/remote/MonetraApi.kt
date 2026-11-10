package com.monetra.app.data.remote

import com.monetra.app.data.remote.dto.AccountDto
import com.monetra.app.data.remote.dto.ApiResponse
import com.monetra.app.data.remote.dto.AuthResponse
import com.monetra.app.data.remote.dto.CategorySuggestionDto
import com.monetra.app.data.remote.dto.CreateAccountRequest
import com.monetra.app.data.remote.dto.CreateDebtRequest
import com.monetra.app.data.remote.dto.CreateSavingsGoalRequest
import com.monetra.app.data.remote.dto.CreateSubscriptionRequest
import com.monetra.app.data.remote.dto.CreateTransactionRequest
import com.monetra.app.data.remote.dto.DashboardSummaryDto
import com.monetra.app.data.remote.dto.DebtDto
import com.monetra.app.data.remote.dto.DepositSavingsGoalRequest
import com.monetra.app.data.remote.dto.LoginRequest
import com.monetra.app.data.remote.dto.MarketRateDto
import com.monetra.app.data.remote.dto.RecordDebtPaymentRequest
import com.monetra.app.data.remote.dto.RegisterRequest
import com.monetra.app.data.remote.dto.SavingsGoalDto
import com.monetra.app.data.remote.dto.SubscriptionDto
import com.monetra.app.data.remote.dto.TransactionDto
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.PATCH
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

interface MonetraApi {

    // --- Auth ---
    @POST("api/auth/register")
    suspend fun register(@Body request: RegisterRequest): ApiResponse<AuthResponse>

    @POST("api/auth/login")
    suspend fun login(@Body request: LoginRequest): ApiResponse<AuthResponse>

    // --- Dashboard ---
    @GET("api/dashboard")
    suspend fun getDashboard(@Query("baseCurrency") baseCurrency: String = "COP"): ApiResponse<DashboardSummaryDto>

    // --- Accounts ---
    @GET("api/accounts")
    suspend fun getAccounts(): ApiResponse<List<AccountDto>>

    @POST("api/accounts")
    suspend fun createAccount(@Body request: CreateAccountRequest): ApiResponse<AccountDto>

    // --- Transactions ---
    @GET("api/transactions")
    suspend fun getTransactions(
        @Query("accountId") accountId: Int? = null,
        @Query("categoryId") categoryId: Int? = null,
        @Query("from") from: String? = null,
        @Query("to") to: String? = null
    ): ApiResponse<List<TransactionDto>>

    @POST("api/transactions")
    suspend fun createTransaction(@Body request: CreateTransactionRequest): ApiResponse<TransactionDto>

    @DELETE("api/transactions/{id}")
    suspend fun deleteTransaction(@Path("id") id: Int): ApiResponse<Boolean>

    // --- AI ---
    @GET("api/ai/suggest-category")
    suspend fun suggestCategory(@Query("description") description: String): ApiResponse<CategorySuggestionDto>

    // --- Market ---
    @GET("api/market/usd-cop")
    suspend fun getUsdCopRate(): ApiResponse<MarketRateDto>

    // --- Debts ---
    @GET("api/debts")
    suspend fun getDebts(): ApiResponse<List<DebtDto>>

    @POST("api/debts")
    suspend fun createDebt(@Body request: CreateDebtRequest): ApiResponse<DebtDto>

    @POST("api/debts/{id}/payments")
    suspend fun recordDebtPayment(@Path("id") id: Int, @Body request: RecordDebtPaymentRequest): ApiResponse<DebtDto>

    @DELETE("api/debts/{id}")
    suspend fun deleteDebt(@Path("id") id: Int): ApiResponse<Boolean>

    // --- Savings Goals ---
    @GET("api/savingsgoals")
    suspend fun getSavingsGoals(): ApiResponse<List<SavingsGoalDto>>

    @POST("api/savingsgoals")
    suspend fun createSavingsGoal(@Body request: CreateSavingsGoalRequest): ApiResponse<SavingsGoalDto>

    @POST("api/savingsgoals/{id}/deposit")
    suspend fun depositToGoal(@Path("id") id: Int, @Body request: DepositSavingsGoalRequest): ApiResponse<SavingsGoalDto>

    @DELETE("api/savingsgoals/{id}")
    suspend fun deleteSavingsGoal(@Path("id") id: Int): ApiResponse<Boolean>

    // --- Subscriptions ---
    @GET("api/subscriptions")
    suspend fun getSubscriptions(): ApiResponse<List<SubscriptionDto>>

    @POST("api/subscriptions")
    suspend fun createSubscription(@Body request: CreateSubscriptionRequest): ApiResponse<SubscriptionDto>

    @PATCH("api/subscriptions/{id}/toggle")
    suspend fun toggleSubscription(@Path("id") id: Int): ApiResponse<SubscriptionDto>

    @DELETE("api/subscriptions/{id}")
    suspend fun deleteSubscription(@Path("id") id: Int): ApiResponse<Boolean>
}
