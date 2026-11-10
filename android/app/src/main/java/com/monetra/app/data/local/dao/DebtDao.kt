package com.monetra.app.data.local.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.monetra.app.data.local.entities.DebtEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface DebtDao {

    @Query("SELECT * FROM debt_table ORDER BY dueDate ASC")
    fun observeAll(): Flow<List<DebtEntity>>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(debts: List<DebtEntity>)

    @Query("DELETE FROM debt_table")
    suspend fun clearAll()
}
