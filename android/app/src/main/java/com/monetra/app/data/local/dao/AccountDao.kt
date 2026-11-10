package com.monetra.app.data.local.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.monetra.app.data.local.entities.AccountEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface AccountDao {

    @Query("SELECT * FROM account_table ORDER BY createdAt ASC")
    fun observeAll(): Flow<List<AccountEntity>>

    @Query("SELECT * FROM account_table ORDER BY createdAt ASC")
    suspend fun getAll(): List<AccountEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(accounts: List<AccountEntity>)

    @Query("UPDATE account_table SET currentBalance = :newBalance WHERE id = :accountId")
    suspend fun updateBalance(accountId: Int, newBalance: java.math.BigDecimal)

    @Query("DELETE FROM account_table")
    suspend fun clearAll()
}
