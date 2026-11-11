package com.monetra.app.ui.market

import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class ChartScaleTest {

    @Test
    fun `ticks are round numbers that cover the whole price range`() {
        val scale = ChartScale.of(min = 213.4f, max = 222.98f)

        assertArrayEquals(floatArrayOf(210f, 215f, 220f, 225f), scale.ticks.toFloatArray(), 0.001f)
        assertEquals(0, scale.decimals)
    }

    @Test
    fun `a narrow price range uses fractional steps with two decimals`() {
        val scale = ChartScale.of(min = 8.02f, max = 8.47f)

        assertArrayEquals(floatArrayOf(8.0f, 8.2f, 8.4f, 8.6f), scale.ticks.toFloatArray(), 0.001f)
        assertEquals(2, scale.decimals)
    }

    @Test
    fun `a flat series still gets a range around its value`() {
        val scale = ChartScale.of(min = 50f, max = 50f)

        assertTrue(scale.low < 50f)
        assertTrue(scale.high > 50f)
    }

    @Test
    fun `fractionOf places a value between the bottom and the top of the scale`() {
        val scale = ChartScale.of(min = 213.4f, max = 222.98f)

        assertEquals(0f, scale.fractionOf(210f), 0.001f)
        assertEquals(1f, scale.fractionOf(225f), 0.001f)
        assertEquals(0.5f, scale.fractionOf(217.5f), 0.001f)
    }
}
