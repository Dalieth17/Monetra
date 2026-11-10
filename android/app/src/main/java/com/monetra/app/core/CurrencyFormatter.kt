package com.monetra.app.core

import java.math.BigDecimal
import java.text.NumberFormat
import java.util.Locale

object CurrencyFormatter {

    fun format(amount: BigDecimal, currencyCode: String): String {
        val locale = if (currencyCode == "USD") Locale.US else Locale("es", "CO")
        val formatter = NumberFormat.getNumberInstance(locale).apply {
            minimumFractionDigits = if (currencyCode == "USD") 2 else 0
            maximumFractionDigits = 2
        }
        return "$${formatter.format(amount)} $currencyCode"
    }
}
