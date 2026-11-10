package com.monetra.app.ui.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val LightColors = lightColorScheme(
    primary = PrimaryGreen,
    onPrimary = Color.White,
    secondary = AccentTeal,
    onSecondary = Color.White,
    background = BackgroundLight,
    onBackground = PrimaryDark,
    surface = Color.White,
    onSurface = PrimaryDark,
    error = ExpenseRed
)

private val DarkColors = darkColorScheme(
    primary = PrimaryGreen,
    onPrimary = Color.White,
    secondary = AccentTeal,
    onSecondary = Color.White,
    background = PrimaryDark,
    onBackground = Color.White,
    surface = Color(0xFF13233B),
    onSurface = Color.White,
    error = ExpenseRed
)

@Composable
fun MonetraTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit
) {
    MaterialTheme(
        colorScheme = if (darkTheme) DarkColors else LightColors,
        typography = MonetraTypography,
        content = content
    )
}
