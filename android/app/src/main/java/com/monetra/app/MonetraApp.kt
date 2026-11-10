package com.monetra.app

import android.app.Application
import com.monetra.app.di.AppContainer

class MonetraApp : Application() {

    lateinit var container: AppContainer
        private set

    override fun onCreate() {
        super.onCreate()
        container = AppContainer(this)
    }
}
