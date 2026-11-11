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

    fun formatDelta(change: BigDecimal, changePercent: BigDecimal): String {
        val sign = when (change.signum()) {
            1 -> "+"
            -1 -> "−"
            else -> ""
        }
        val amount = String.format(Locale.US, "%.2f", change.abs())
        val percent = String.format(Locale.US, "%.2f", changePercent.abs())
        return "$sign$amount ($sign$percent%)"
    }
}
