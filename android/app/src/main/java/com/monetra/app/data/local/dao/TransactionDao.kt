package com.monetra.app.data.local.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.monetra.app.data.local.entities.TransactionEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface TransactionDao {

    @Query("SELECT * FROM transaction_table ORDER BY date DESC, localId DESC")
    fun observeAll(): Flow<List<TransactionEntity>>

    @Query("SELECT * FROM transaction_table WHERE isSynced = 0 ORDER BY createdAt ASC")
    suspend fun getPendingSync(): List<TransactionEntity>

    @Query("SELECT COUNT(*) FROM transaction_table WHERE isSynced = 0")
    fun observePendingCount(): Flow<Int>

    @Query("SELECT * FROM transaction_table WHERE date >= :fromMillis ORDER BY date DESC")
    suspend fun getSince(fromMillis: Long): List<TransactionEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(transaction: TransactionEntity): Long

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(transactions: List<TransactionEntity>)

    @Query("UPDATE transaction_table SET isSynced = 1, serverId = :serverId WHERE localId = :localId")
    suspend fun markSynced(localId: Long, serverId: Int)

    @Query("DELETE FROM transaction_table WHERE isSynced = 1")
    suspend fun clearSynced()
}
