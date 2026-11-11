package com.monetra.app.data.remote

import com.monetra.app.data.remote.dto.CreateTransactionRequest
import com.monetra.app.data.remote.dto.DepositSavingsGoalRequest
import com.monetra.app.data.remote.dto.RecordDebtPaymentRequest
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test
import java.math.BigDecimal

class DemoApiTest {

    private fun demo() = DemoApi()

    @Test
    fun `dashboard seeds COP and USD accounts with sample movements`() = runBlocking {
        val summary = demo().getDashboard("COP").data!!
        assertEquals(listOf("COP", "USD"), summary.accounts.map { it.currencyCode }.sorted())
        assertTrue(summary.recentTransactions.isNotEmpty())
    }

    @Test
    fun `dashboard totals USD accounts into COP at the demo rate`() = runBlocking {
        // 1.850.000 COP + 420 USD * 4150 = 1.850.000 + 1.743.000
        val summary = demo().getDashboard("COP").data!!
        assertEquals(BigDecimal("3593000.00"), summary.totalNetWorth)
    }

    @Test
    fun `creating an expense lowers the account balance`() = runBlocking {
        val api = demo()
        val before = api.getAccounts().data!!.first { it.id == 1 }.currentBalance
        api.createTransaction(CreateTransactionRequest(1, 1, BigDecimal("90000"), 2, null, "Mercado"))
        val after = api.getAccounts().data!!.first { it.id == 1 }.currentBalance
        assertEquals(before.subtract(BigDecimal("90000")), after)
    }

    @Test
    fun `creating a transaction on a missing account is rejected and changes nothing`() = runBlocking {
        val api = demo()
        val before = api.getAccounts().data!!.first { it.id == 1 }.currentBalance
        val response = api.createTransaction(CreateTransactionRequest(999, null, BigDecimal("90000"), 2, null, "x"))
        assertFalse(response.success)
        assertNull(response.data)
        assertEquals(before, api.getAccounts().data!!.first { it.id == 1 }.currentBalance)
    }

    @Test
    fun `suggestCategory classifies by keyword`() = runBlocking {
        assertEquals("Alimentación", demo().suggestCategory("Mercado en Éxito").data!!.categoryName)
        assertEquals("Transporte", demo().suggestCategory("Viaje en Uber").data!!.categoryName)
        assertEquals("Otros", demo().suggestCategory("xyzabc").data!!.categoryName)
    }

    @Test
    fun `depositing until the target completes the goal and charges the account`() = runBlocking {
        val api = demo()
        // Meta "Viaje a San Andrés": 650.000 de 2.000.000.
        val goalId = api.getSavingsGoals().data!!.first { it.name == "Viaje a San Andrés" }.id
        val balanceBefore = api.getAccounts().data!!.first { it.id == 1 }.currentBalance

        val updated = api.depositToGoal(goalId, DepositSavingsGoalRequest(1, BigDecimal("1350000"))).data!!

        assertTrue(updated.isCompleted)
        assertEquals(100.0, updated.progressPercentage, 0.001)
        assertEquals(balanceBefore.subtract(BigDecimal("1350000")), api.getAccounts().data!!.first { it.id == 1 }.currentBalance)
    }

    @Test
    fun `recording a debt payment never leaves a negative balance`() = runBlocking {
        val api = demo()
        val debtId = api.getDebts().data!!.first().id
        val updated = api.recordDebtPayment(debtId, RecordDebtPaymentRequest(1, BigDecimal("9999999"))).data!!
        assertEquals(BigDecimal.ZERO, updated.remainingAmount)
    }

    @Test
    fun `reset restores the sample data after changes`() = runBlocking {
        val api = demo()
        api.deleteDebt(api.getDebts().data!!.first().id)
        assertTrue(api.getDebts().data!!.isEmpty())
        api.reset()
        assertEquals(1, api.getDebts().data!!.size)
    }

    @Test
    fun `each stock exposes a one-month daily history`() = runBlocking {
        val stocks = demo().getStocks().data!!
        assertEquals(4, stocks.size)
        assertTrue(stocks.all { it.history.size == 22 })
        val apple = stocks.first { it.symbol == "AAPL" }
        assertEquals(apple.price.subtract(apple.previousClose), apple.change)
    }
}
