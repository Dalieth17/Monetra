package com.monetra.app.data.local

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import androidx.room.TypeConverters
import com.monetra.app.data.local.converters.Converters
import com.monetra.app.data.local.dao.AccountDao
import com.monetra.app.data.local.dao.DebtDao
import com.monetra.app.data.local.dao.SavingsGoalDao
import com.monetra.app.data.local.dao.SubscriptionDao
import com.monetra.app.data.local.dao.TransactionDao
import com.monetra.app.data.local.entities.AccountEntity
import com.monetra.app.data.local.entities.DebtEntity
import com.monetra.app.data.local.entities.SavingsGoalEntity
import com.monetra.app.data.local.entities.SubscriptionEntity
import com.monetra.app.data.local.entities.TransactionEntity

@Database(
    entities = [
        AccountEntity::class,
        TransactionEntity::class,
        DebtEntity::class,
        SavingsGoalEntity::class,
        SubscriptionEntity::class
    ],
    version = 1,
    exportSchema = false
)
@TypeConverters(Converters::class)
abstract class MonetraDatabase : RoomDatabase() {

    abstract fun accountDao(): AccountDao
    abstract fun transactionDao(): TransactionDao
    abstract fun debtDao(): DebtDao
    abstract fun savingsGoalDao(): SavingsGoalDao
    abstract fun subscriptionDao(): SubscriptionDao

    companion object {
        @Volatile
        private var INSTANCE: MonetraDatabase? = null

        fun getInstance(context: Context): MonetraDatabase {
            return INSTANCE ?: synchronized(this) {
                INSTANCE ?: Room.databaseBuilder(
                    context.applicationContext,
                    MonetraDatabase::class.java,
                    "monetra_db"
                )
                    .fallbackToDestructiveMigration()
                    .build()
                    .also { INSTANCE = it }
            }
        }
    }
}
