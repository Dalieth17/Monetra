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
    fun `format COP includes currency code`() {
        val result = CurrencyFormatter.format(BigDecimal("50000"), "COP")
        assertTrue(result.endsWith("COP"))
        assertTrue(result.startsWith("$"))
    }
}
