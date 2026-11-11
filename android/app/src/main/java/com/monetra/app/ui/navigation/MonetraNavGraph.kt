package com.monetra.app.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.monetra.app.ui.auth.LoginScreen
import com.monetra.app.ui.auth.RegisterScreen
import com.monetra.app.ui.dashboard.DashboardScreen
import com.monetra.app.ui.debts.DebtsScreen
import com.monetra.app.ui.market.MarketScreen
import com.monetra.app.ui.savings.SavingsGoalsScreen
import com.monetra.app.ui.subscriptions.SubscriptionsScreen
import com.monetra.app.ui.transactions.AddTransactionScreen

object Routes {
    const val LOGIN = "login"
    const val REGISTER = "register"
    const val DASHBOARD = "dashboard"
    const val ADD_TRANSACTION = "add_transaction"
    const val DEBTS = "debts"
    const val SAVINGS = "savings"
    const val SUBSCRIPTIONS = "subscriptions"
    const val MARKET = "market"
}

@Composable
fun MonetraNavGraph(isLoggedIn: Boolean) {
    val navController = rememberNavController()
    val startDestination = if (isLoggedIn) Routes.DASHBOARD else Routes.LOGIN

    NavHost(navController = navController, startDestination = startDestination) {

        composable(Routes.LOGIN) {
            LoginScreen(
                onLoginSuccess = {
                    navController.navigate(Routes.DASHBOARD) {
                        popUpTo(Routes.LOGIN) { inclusive = true }
                    }
                },
                onNavigateToRegister = { navController.navigate(Routes.REGISTER) }
            )
        }

        composable(Routes.REGISTER) {
            RegisterScreen(
                onRegisterSuccess = {
                    navController.navigate(Routes.DASHBOARD) {
                        popUpTo(Routes.LOGIN) { inclusive = true }
                    }
                },
                onNavigateBack = { navController.popBackStack() }
            )
        }

        composable(Routes.DASHBOARD) {
            DashboardScreen(
                onAddTransaction = { navController.navigate(Routes.ADD_TRANSACTION) },
                onOpenDebts = { navController.navigate(Routes.DEBTS) },
                onOpenSavings = { navController.navigate(Routes.SAVINGS) },
                onOpenSubscriptions = { navController.navigate(Routes.SUBSCRIPTIONS) },
                onOpenMarket = { navController.navigate(Routes.MARKET) },
                onLogout = {
                    navController.navigate(Routes.LOGIN) {
                        popUpTo(Routes.DASHBOARD) { inclusive = true }
                    }
                }
            )
        }

        composable(Routes.ADD_TRANSACTION) {
            AddTransactionScreen(onDone = { navController.popBackStack() })
        }

        composable(Routes.DEBTS) {
            DebtsScreen(onNavigateBack = { navController.popBackStack() })
        }

        composable(Routes.SAVINGS) {
            SavingsGoalsScreen(onNavigateBack = { navController.popBackStack() })
        }

        composable(Routes.SUBSCRIPTIONS) {
            SubscriptionsScreen(onNavigateBack = { navController.popBackStack() })
        }

        composable(Routes.MARKET) {
            MarketScreen(onNavigateBack = { navController.popBackStack() })
        }
    }
}
