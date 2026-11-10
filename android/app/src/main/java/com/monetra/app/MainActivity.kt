package com.monetra.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.monetra.app.ui.navigation.MonetraNavGraph
import com.monetra.app.ui.theme.MonetraTheme

class MainActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        val isLoggedIn = (application as MonetraApp).container.sessionManager.isLoggedIn()
        setContent {
            MonetraTheme {
                MonetraNavGraph(isLoggedIn = isLoggedIn)
            }
        }
    }
}
