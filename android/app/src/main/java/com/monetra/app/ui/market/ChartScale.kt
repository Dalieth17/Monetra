package com.monetra.app.ui.market

import kotlin.math.ceil
import kotlin.math.floor
import kotlin.math.log10
import kotlin.math.pow

/** Eje Y con marcas redondas que cubren todo el rango de precios. */
class ChartScale private constructor(
    val low: Float,
    val high: Float,
    val ticks: List<Float>,
    val decimals: Int
) {
    fun fractionOf(value: Float): Float = (value - low) / (high - low)

    companion object {
        fun of(min: Float, max: Float): ChartScale {
            var lower = min
            var upper = max
            if (lower == upper) {
                lower -= 1f
                upper += 1f
            }
            val rough = (upper - lower) / 3f
            val magnitude = 10f.pow(floor(log10(rough)))
            val step = listOf(1f, 2f, 2.5f, 5f, 10f).map { it * magnitude }.first { it >= rough }
            val low = floor(lower / step) * step
            val high = ceil(upper / step) * step
            val ticks = generateSequence(low) { it + step }
                .takeWhile { it <= high + step / 2 }
                .toList()
            val decimals = when {
                step >= 1f -> if (step == floor(step)) 0 else 1
                else -> 2
            }
            return ChartScale(low, high, ticks, decimals)
        }
    }
}
