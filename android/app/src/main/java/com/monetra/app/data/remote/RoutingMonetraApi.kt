package com.monetra.app.data.remote

import com.monetra.app.data.local.SessionManager
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
import com.monetra.app.data.remote.dto.StockQuoteDto
import com.monetra.app.data.remote.dto.SubscriptionDto
import com.monetra.app.data.remote.dto.TransactionDto

/**
 * Enruta cada llamada al backend real o al [DemoApi] según la bandera de modo
 * demostración. Así los repositorios usan una sola [MonetraApi] sin saber en qué
 * modo están, y el cambio ocurre en un único lugar.
 */
class RoutingMonetraApi(
    private val real: MonetraApi,
    private val demo: DemoApi,
    private val sessionManager: SessionManager
) : MonetraApi {

    private fun api(): MonetraApi = if (sessionManager.isDemo) demo else real

    override suspend fun register(request: RegisterRequest): ApiResponse<AuthResponse> = api().register(request)
    override suspend fun login(request: LoginRequest): ApiResponse<AuthResponse> = api().login(request)
    override suspend fun getDashboard(baseCurrency: String): ApiResponse<DashboardSummaryDto> = api().getDashboard(baseCurrency)
    override suspend fun getAccounts(): ApiResponse<List<AccountDto>> = api().getAccounts()
    override suspend fun createAccount(request: CreateAccountRequest): ApiResponse<AccountDto> = api().createAccount(request)
    override suspend fun getTransactions(accountId: Int?, categoryId: Int?, from: String?, to: String?): ApiResponse<List<TransactionDto>> =
        api().getTransactions(accountId, categoryId, from, to)
    override suspend fun createTransaction(request: CreateTransactionRequest): ApiResponse<TransactionDto> = api().createTransaction(request)
    override suspend fun deleteTransaction(id: Int): ApiResponse<Boolean> = api().deleteTransaction(id)
    override suspend fun suggestCategory(description: String): ApiResponse<CategorySuggestionDto> = api().suggestCategory(description)
    override suspend fun getUsdCopRate(): ApiResponse<MarketRateDto> = api().getUsdCopRate()
    override suspend fun getStocks(): ApiResponse<List<StockQuoteDto>> = api().getStocks()
    override suspend fun getDebts(): ApiResponse<List<DebtDto>> = api().getDebts()
    override suspend fun createDebt(request: CreateDebtRequest): ApiResponse<DebtDto> = api().createDebt(request)
    override suspend fun recordDebtPayment(id: Int, request: RecordDebtPaymentRequest): ApiResponse<DebtDto> = api().recordDebtPayment(id, request)
    override suspend fun deleteDebt(id: Int): ApiResponse<Boolean> = api().deleteDebt(id)
    override suspend fun getSavingsGoals(): ApiResponse<List<SavingsGoalDto>> = api().getSavingsGoals()
    override suspend fun createSavingsGoal(request: CreateSavingsGoalRequest): ApiResponse<SavingsGoalDto> = api().createSavingsGoal(request)
    override suspend fun depositToGoal(id: Int, request: DepositSavingsGoalRequest): ApiResponse<SavingsGoalDto> = api().depositToGoal(id, request)
    override suspend fun deleteSavingsGoal(id: Int): ApiResponse<Boolean> = api().deleteSavingsGoal(id)
    override suspend fun getSubscriptions(): ApiResponse<List<SubscriptionDto>> = api().getSubscriptions()
    override suspend fun createSubscription(request: CreateSubscriptionRequest): ApiResponse<SubscriptionDto> = api().createSubscription(request)
    override suspend fun toggleSubscription(id: Int): ApiResponse<SubscriptionDto> = api().toggleSubscription(id)
    override suspend fun deleteSubscription(id: Int): ApiResponse<Boolean> = api().deleteSubscription(id)
}
