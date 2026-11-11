package com.monetra.app.data.remote

import com.monetra.app.core.DateUtils
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
import com.monetra.app.data.remote.dto.StockPricePointDto
import com.monetra.app.data.remote.dto.StockQuoteDto
import com.monetra.app.data.remote.dto.SubscriptionDto
import com.monetra.app.data.remote.dto.TransactionDto
import kotlinx.coroutines.delay
import java.math.BigDecimal
import java.math.RoundingMode
import java.util.concurrent.atomic.AtomicInteger
import kotlin.math.max

/**
 * Implementación de [MonetraApi] con datos de ejemplo en memoria, para el modo
 * demostración: permite recorrer toda la app sin el backend .NET. Crear, abonar,
 * depositar o borrar actualiza el estado en vivo, igual que contra el servidor real.
 */
class DemoApi : MonetraApi {

    private val rate = BigDecimal("4150.00")
    private val nextId = AtomicInteger(100)

    private val accounts = mutableListOf<AccountDto>()
    private val transactions = mutableListOf<TransactionDto>()
    private val debts = mutableListOf<DebtDto>()
    private val goals = mutableListOf<SavingsGoalDto>()
    private val subscriptions = mutableListOf<SubscriptionDto>()
    private val stocks = mutableListOf<StockQuoteDto>()

    init {
        reset()
    }

    /** Reinicia los datos de ejemplo a su estado inicial (al entrar en modo demo). */
    @Synchronized
    fun reset() {
        nextId.set(100)
        accounts.clear(); transactions.clear(); debts.clear(); goals.clear(); subscriptions.clear(); stocks.clear()

        accounts += AccountDto(1, "Efectivo COP", "COP", BigDecimal("1850000"), BigDecimal("1850000"), iso(40))
        accounts += AccountDto(2, "Cuenta Banco USD", "USD", BigDecimal("420"), BigDecimal("1743000"), iso(40))

        transactions += tx(1, "Salario quincena", BigDecimal("2200000"), 1, 9, iso(8))
        transactions += tx(1, "Mercado en Éxito", BigDecimal("185000"), 2, 1, iso(6))
        transactions += tx(1, "Mensualidad de Netflix", BigDecimal("38900"), 2, 8, iso(4))
        transactions += tx(2, "Suscripción ChatGPT", BigDecimal("20"), 2, 8, iso(3))
        transactions += tx(1, "Viaje en Uber", BigDecimal("23500"), 2, 2, iso(1))

        debts += DebtDto(1, "Bancolombia", BigDecimal("5000000"), BigDecimal("3200000"), BigDecimal("1.5"), "EM", iso(-540), BigDecimal("295000"), iso(60))

        goals += goal(1, "Viaje a San Andrés", BigDecimal("2000000"), BigDecimal("650000"), iso(-150))
        goals += goal(2, "Fondo de emergencia", BigDecimal("5000000"), BigDecimal("5000000"), iso(-300))

        subscriptions += SubscriptionDto(1, "Netflix", BigDecimal("38900"), "COP", "Monthly", iso(-2), true, 2, iso(40))
        subscriptions += SubscriptionDto(2, "Spotify", BigDecimal("16900"), "COP", "Monthly", iso(-15), true, 15, iso(40))

        stocks += stock("AAPL", "Apple Inc.", 221.0, 0.002)
        stocks += stock("MSFT", "Microsoft Corporation", 430.0, -0.001)
        stocks += stock("NVDA", "NVIDIA Corporation", 178.0, 0.004)
        stocks += stock("EC", "Ecopetrol S.A.", 8.1, 0.0015)
    }

    override suspend fun register(request: RegisterRequest): ApiResponse<AuthResponse> = enterResponse()
    override suspend fun login(request: LoginRequest): ApiResponse<AuthResponse> = enterResponse()

    override suspend fun getDashboard(baseCurrency: String): ApiResponse<DashboardSummaryDto> {
        latency()
        val base = baseCurrency.uppercase()
        val monthStart = DateUtils.startOfCurrentMonthMillis()
        var income = BigDecimal.ZERO
        var expense = BigDecimal.ZERO
        transactions.forEach { t ->
            if (DateUtils.parseIsoToMillis(t.date) >= monthStart) {
                val converted = convert(t.amount, t.currencyCode, base)
                if (t.type == 1) income = income.add(converted) else expense = expense.add(converted)
            }
        }
        val netWorth = accounts.fold(BigDecimal.ZERO) { acc, a -> acc.add(convert(a.currentBalance, a.currencyCode, base)) }
        val summary = DashboardSummaryDto(
            baseCurrency = base,
            totalNetWorth = netWorth.setScale(2, RoundingMode.HALF_UP),
            monthlyIncome = income.setScale(2, RoundingMode.HALF_UP),
            monthlyExpense = expense.setScale(2, RoundingMode.HALF_UP),
            netSavingsThisMonth = income.subtract(expense).setScale(2, RoundingMode.HALF_UP),
            usdToCopExchangeRate = rate,
            exchangeRateTimestamp = iso(0),
            isRateCached = false,
            accounts = accounts.map { it.copy(convertedBalance = convert(it.currentBalance, it.currencyCode, base).setScale(2, RoundingMode.HALF_UP)) },
            recentTransactions = transactions.sortedByDescending { it.date }.take(10)
        )
        return ok(summary)
    }

    override suspend fun getAccounts(): ApiResponse<List<AccountDto>> = ok(accounts.toList())

    override suspend fun createAccount(request: CreateAccountRequest): ApiResponse<AccountDto> {
        val account = AccountDto(nextId.getAndIncrement(), request.name.trim(), request.currencyCode.uppercase(), request.initialBalance, request.initialBalance, iso(0))
        accounts += account
        return ok(account)
    }

    override suspend fun getTransactions(accountId: Int?, categoryId: Int?, from: String?, to: String?): ApiResponse<List<TransactionDto>> =
        ok(transactions.sortedByDescending { it.date })

    override suspend fun createTransaction(request: CreateTransactionRequest): ApiResponse<TransactionDto> {
        latency()
        val index = accounts.indexOfFirst { it.id == request.accountId }
        if (index < 0) return notFound("Cuenta")
        val account = accounts[index]
        val newBalance = if (request.type == 1) account.currentBalance.add(request.amount) else account.currentBalance.subtract(request.amount)
        accounts[index] = account.copy(currentBalance = newBalance)
        val category = CATEGORIES[request.categoryId]
        val created = TransactionDto(
            id = nextId.getAndIncrement(), accountId = account.id, accountName = account.name, currencyCode = account.currencyCode,
            categoryId = request.categoryId, categoryName = category?.first ?: "Sin categoría", categoryIcon = category?.second ?: "more_horiz",
            amount = request.amount, type = request.type, date = request.date ?: iso(0), description = request.description, createdAt = iso(0)
        )
        transactions += created
        return ok(created)
    }

    override suspend fun deleteTransaction(id: Int): ApiResponse<Boolean> {
        transactions.removeAll { it.id == id }
        return ok(true)
    }

    override suspend fun suggestCategory(description: String): ApiResponse<CategorySuggestionDto> {
        latency()
        val text = description.lowercase()
        for ((name, keywords) in KEYWORDS) {
            val hit = keywords.firstOrNull { text.contains(it) }
            if (hit != null) {
                val id = CATEGORIES.entries.first { it.value.first == name }.key
                return ok(CategorySuggestionDto(id, name, if (hit.length > 4) 0.95 else 0.9, "Patrón \"$hit\""))
            }
        }
        return ok(CategorySuggestionDto(11, "Otros", 0.30, "Clasificación general por defecto"))
    }

    override suspend fun getUsdCopRate(): ApiResponse<MarketRateDto> =
        ok(MarketRateDto("USD", "COP", rate, iso(0), false, ""))

    override suspend fun getStocks(): ApiResponse<List<StockQuoteDto>> {
        latency()
        return ok(stocks.toList())
    }

    override suspend fun getDebts(): ApiResponse<List<DebtDto>> = ok(debts.sortedBy { it.dueDate })

    override suspend fun createDebt(request: CreateDebtRequest): ApiResponse<DebtDto> {
        val debt = DebtDto(
            nextId.getAndIncrement(), request.creditor.trim(), request.totalAmount, request.totalAmount,
            request.interestRate, request.rateType.uppercase(), request.dueDate,
            request.totalAmount.divide(BigDecimal(12), 2, RoundingMode.HALF_UP), iso(0)
        )
        debts += debt
        return ok(debt)
    }

    override suspend fun recordDebtPayment(id: Int, request: RecordDebtPaymentRequest): ApiResponse<DebtDto> {
        val debtIndex = debts.indexOfFirst { it.id == id }
        val accountIndex = accounts.indexOfFirst { it.id == request.accountId }
        if (debtIndex < 0) return notFound("Deuda")
        if (accountIndex < 0) return notFound("Cuenta")
        val debt = debts[debtIndex]
        val remaining = debt.remainingAmount.subtract(request.amount).max(BigDecimal.ZERO)
        debts[debtIndex] = debt.copy(remainingAmount = remaining)
        accounts[accountIndex] = accounts[accountIndex].let { it.copy(currentBalance = it.currentBalance.subtract(request.amount)) }
        transactions += tx(request.accountId, "Abono a deuda: ${debt.creditor}", request.amount, 2, 11, iso(0))
        return ok(debts[debtIndex])
    }

    override suspend fun deleteDebt(id: Int): ApiResponse<Boolean> {
        debts.removeAll { it.id == id }
        return ok(true)
    }

    override suspend fun getSavingsGoals(): ApiResponse<List<SavingsGoalDto>> = ok(goals.sortedBy { it.deadline })

    override suspend fun createSavingsGoal(request: CreateSavingsGoalRequest): ApiResponse<SavingsGoalDto> {
        val created = goal(nextId.getAndIncrement(), request.name.trim(), request.targetAmount, BigDecimal.ZERO, request.deadline)
        goals += created
        return ok(created)
    }

    override suspend fun depositToGoal(id: Int, request: DepositSavingsGoalRequest): ApiResponse<SavingsGoalDto> {
        val goalIndex = goals.indexOfFirst { it.id == id }
        val accountIndex = accounts.indexOfFirst { it.id == request.accountId }
        if (goalIndex < 0) return notFound("Meta de ahorro")
        if (accountIndex < 0) return notFound("Cuenta")
        val existing = goals[goalIndex]
        val updated = goal(existing.id, existing.name, existing.targetAmount, existing.currentAmount.add(request.amount), existing.deadline)
        goals[goalIndex] = updated
        accounts[accountIndex] = accounts[accountIndex].let { it.copy(currentBalance = it.currentBalance.subtract(request.amount)) }
        transactions += tx(request.accountId, "Depósito a meta: ${existing.name}", request.amount, 2, 10, iso(0))
        return ok(updated)
    }

    override suspend fun deleteSavingsGoal(id: Int): ApiResponse<Boolean> {
        goals.removeAll { it.id == id }
        return ok(true)
    }

    override suspend fun getSubscriptions(): ApiResponse<List<SubscriptionDto>> = ok(subscriptions.sortedBy { it.nextPaymentDate })

    override suspend fun createSubscription(request: CreateSubscriptionRequest): ApiResponse<SubscriptionDto> {
        val days = max(0L, (DateUtils.parseIsoToMillis(request.nextPaymentDate) - System.currentTimeMillis()) / 86_400_000L).toInt()
        val created = SubscriptionDto(
            nextId.getAndIncrement(), request.name.trim(), request.amount, request.currencyCode.uppercase(),
            request.billingCycle, request.nextPaymentDate, true, days, iso(0)
        )
        subscriptions += created
        return ok(created)
    }

    override suspend fun toggleSubscription(id: Int): ApiResponse<SubscriptionDto> {
        val index = subscriptions.indexOfFirst { it.id == id }
        if (index < 0) return notFound("Suscripción")
        subscriptions[index] = subscriptions[index].let { it.copy(isActive = !it.isActive) }
        return ok(subscriptions[index])
    }

    override suspend fun deleteSubscription(id: Int): ApiResponse<Boolean> {
        subscriptions.removeAll { it.id == id }
        return ok(true)
    }

    // ---------- Helpers ----------
    private suspend fun latency() = delay(180)
    private fun <T> ok(data: T): ApiResponse<T> = ApiResponse(success = true, data = data)
    private fun <T> notFound(entity: String): ApiResponse<T> =
        ApiResponse(success = false, message = "La entidad \"$entity\" no fue encontrada.", data = null)

    private fun enterResponse() = ok(AuthResponse("demo", 1, "Invitado Demo", "demo@monetra.com", "Usuario"))

    private fun iso(daysAgo: Int): String = DateUtils.millisToIso(System.currentTimeMillis() - daysAgo * 86_400_000L)

    private fun convert(amount: BigDecimal, from: String, to: String): BigDecimal {
        if (from == to) return amount
        return when {
            from == "USD" && to == "COP" -> amount.multiply(rate)
            from == "COP" && to == "USD" -> amount.divide(rate, 2, RoundingMode.HALF_UP)
            else -> amount
        }
    }

    private fun tx(accountId: Int, description: String, amount: BigDecimal, type: Int, categoryId: Int, date: String): TransactionDto {
        val account = accounts.first { it.id == accountId }
        val category = CATEGORIES[categoryId]
        return TransactionDto(
            nextId.getAndIncrement(), accountId, account.name, account.currencyCode,
            categoryId, category?.first ?: "Sin categoría", category?.second ?: "more_horiz",
            amount, type, date, description, date
        )
    }

    private fun goal(id: Int, name: String, target: BigDecimal, current: BigDecimal, deadline: String): SavingsGoalDto {
        val progress = if (target > BigDecimal.ZERO)
            current.divide(target, 4, RoundingMode.HALF_UP).multiply(BigDecimal(100)).toDouble().coerceAtMost(100.0)
        else 0.0
        return SavingsGoalDto(id, name, target, current, Math.round(progress * 100) / 100.0, deadline, current >= target, iso(30))
    }

    private fun stock(symbol: String, name: String, start: Double, drift: Double): StockQuoteDto {
        val history = mutableListOf<StockPricePointDto>()
        var value = start
        var seed = symbol[0].code * 7 + symbol.length
        var dayOffset = 32
        while (history.size < 22) {
            val millis = System.currentTimeMillis() - dayOffset * 86_400_000L
            val dow = ((millis / 86_400_000L + 4) % 7).toInt() // 0 = domingo
            if (dow != 0 && dow != 6) {
                seed = (seed * 9301 + 49297) % 233280
                value *= 1 + drift + (seed.toDouble() / 233280 - 0.5) * 0.03
                history += StockPricePointDto(DateUtils.millisToIso(millis), BigDecimal(value).setScale(2, RoundingMode.HALF_UP))
            }
            dayOffset--
        }
        val price = history.last().close
        val previousClose = history[history.size - 2].close
        val change = price.subtract(previousClose)
        val percent = change.divide(previousClose, 4, RoundingMode.HALF_UP).multiply(BigDecimal(100)).setScale(2, RoundingMode.HALF_UP)
        return StockQuoteDto(symbol, name, price, previousClose, change, percent, "USD", history.last().date, false, "", history)
    }

    private companion object {
        val CATEGORIES = mapOf(
            1 to ("Alimentación" to "restaurant"), 2 to ("Transporte" to "directions_bus"),
            3 to ("Vivienda" to "home"), 4 to ("Salud" to "local_hospital"),
            5 to ("Entretenimiento" to "movie"), 6 to ("Educación" to "school"),
            7 to ("Servicios Básicos" to "bolt"), 8 to ("Suscripciones" to "subscriptions"),
            9 to ("Salario" to "payments"), 10 to ("Inversiones" to "trending_up"),
            11 to ("Otros" to "more_horiz")
        )
        val KEYWORDS = linkedMapOf(
            "Alimentación" to listOf("éxito", "exito", "carulla", "d1", "ara", "supermercado", "restaurante", "almuerzo", "mercado", "rappi", "café", "cafe", "comida"),
            "Transporte" to listOf("uber", "didi", "gasolina", "taxi", "bus", "transmilenio", "peaje", "vuelo", "avianca"),
            "Servicios Básicos" to listOf("enel", "epm", "claro", "tigo", "movistar", "internet", "agua", "luz", "recibo", "gas"),
            "Salud" to listOf("droguería", "drogueria", "farmacia", "médico", "medico", "eps", "hospital"),
            "Entretenimiento" to listOf("cine", "concierto", "steam", "bar", "cerveza", "teatro"),
            "Suscripciones" to listOf("netflix", "spotify", "youtube", "disney", "apple", "chatgpt", "openai"),
            "Educación" to listOf("universidad", "curso", "udemy", "platzi", "libros", "matricula"),
            "Salario" to listOf("salario", "sueldo", "nomina", "nómina", "quincena", "prima"),
            "Inversiones" to listOf("dividendo", "acciones", "cripto", "binance", "cdt")
        )
    }
}
