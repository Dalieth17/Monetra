package com.monetra.app.core

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import java.math.BigDecimal

class CurrencyFormatterTest {

    @Test
    fun `format USD uses two decimals and thousands separator`() {
        val result = CurrencyFormatter.format(BigDecimal("1234.5"), "USD")
        assertEquals("$1,234.50 USD", result)
    }

    @Test
    fun `formatDelta marks a rise with a plus sign`() {
        assertEquals("+1.29 (+0.57%)", CurrencyFormatter.formatDelta(BigDecimal("1.29"), BigDecimal("0.57")))
    }

    @Test
    fun `formatDelta marks a fall with a minus sign`() {
        assertEquals("−4.97 (−1.24%)", CurrencyFormatter.formatDelta(BigDecimal("-4.97"), BigDecimal("-1.24")))
    }

    @Test
    fun `formatDelta shows no sign when the price did not move`() {
        assertEquals("0.00 (0.00%)", CurrencyFormatter.formatDelta(BigDecimal.ZERO, BigDecimal.ZERO))
    }

    @Test
    fun `formatDelta always shows two decimals`() {
        assertEquals("+1.50 (+0.60%)", CurrencyFormatter.formatDelta(BigDecimal("1.5"), BigDecimal("0.6")))
    }

    @Test
    fun `format COP includes currency code`() {
        val result = CurrencyFormatter.format(BigDecimal("50000"), "COP")
        assertTrue(result.endsWith("COP"))
        assertTrue(result.startsWith("$"))
    }
}
