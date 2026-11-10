package com.monetra.app.data.sync

import com.monetra.app.core.NetworkMonitor
import com.monetra.app.data.local.dao.TransactionDao
import com.monetra.app.data.remote.MonetraApi
import com.monetra.app.data.remote.dto.CreateTransactionRequest
import com.monetra.app.data.remote.safeApiCall
import com.monetra.app.core.DateUtils

/**
 * Sincroniza las transacciones guardadas offline (isSynced = false) con la API.
 * Se invoca al cargar el Dashboard cuando hay conectividad.
 */
class SyncManager(
    private val api: MonetraApi,
    private val transactionDao: TransactionDao,
    private val networkMonitor: NetworkMonitor
) {

    suspend fun syncPendingTransactions(): Int {
        if (!networkMonitor.isOnline()) return 0

        val pending = transactionDao.getPendingSync()
        var syncedCount = 0

        pending.forEach { local ->
            val result = safeApiCall {
                api.createTransaction(
                    CreateTransactionRequest(
                        accountId = local.accountId,
                        categoryId = local.categoryId,
                        amount = local.amount,
                        type = local.type,
                        date = DateUtils.millisToIso(local.date),
                        description = local.description
                    )
                )
            }
            result.onSuccess { dto ->
                transactionDao.markSynced(local.localId, dto.id)
                syncedCount++
            }
        }
        return syncedCount
    }
}
