package com.monetra.app.core

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class DateUtilsTest {

    @Test
    fun `parseIsoToMillis and millisToIso are inverse operations`() {
        val iso = "2026-01-15T10:30:00"
        val millis = DateUtils.parseIsoToMillis(iso)
        assertEquals(iso, DateUtils.millisToIso(millis))
    }

    @Test
    fun `parseIsoToMillis truncates backend fractional seconds`() {
        val withFraction = "2026-01-15T10:30:00.1234567"
        val clean = "2026-01-15T10:30:00"
        assertEquals(DateUtils.parseIsoToMillis(clean), DateUtils.parseIsoToMillis(withFraction))
    }

    @Test
    fun `parseIsoToMillis falls back to now on invalid input`() {
        val before = System.currentTimeMillis()
        val result = DateUtils.parseIsoToMillis("fecha-invalida")
        assertTrue(result >= before)
    }

    @Test
    fun `startOfCurrentMonthMillis is before now`() {
        assertTrue(DateUtils.startOfCurrentMonthMillis() <= System.currentTimeMillis())
    }
}
