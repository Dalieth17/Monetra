import { useEffect, useRef, useState } from 'react';
import Layout from '../components/Layout.jsx';
import { Icon } from '../components/Icon.jsx';
import { Dialog, EmptyState, ErrorBanner, Shimmer, parseAmount } from '../components/common.jsx';
import { TransactionRow } from './Dashboard.jsx';
import { useAsync } from '../lib/useAsync.js';
import { Api } from '../api/index.js';
import { useToast } from '../components/Toast.jsx';

function AddTransactionDialog({ accounts, onClose, onSaved }) {
  const toast = useToast();
  const [type, setType] = useState(2);
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [accountId, setAccountId] = useState(accounts[0]?.id ?? '');
  const [suggestion, setSuggestion] = useState(null);
  const [accepted, setAccepted] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const timer = useRef(null);

  const value = parseAmount(amount);
  const amountOk = value != null && value > 0;
  const canSave = amountOk && accountId && !saving;

  useEffect(() => {
    setSuggestion(null);
    setAccepted(false);
    clearTimeout(timer.current);
    if (description.trim().length < 4) return;
    timer.current = setTimeout(async () => {
      try { setSuggestion(await Api.suggestCategory(description)); } catch { /* opcional */ }
    }, 500);
    return () => clearTimeout(timer.current);
  }, [description]);

  const save = async () => {
    setSaving(true);
    setError(null);
    try {
      await Api.createTransaction({
        accountId: Number(accountId),
        categoryId: accepted ? suggestion?.categoryId ?? null : null,
        amount: value,
        type,
        date: null,
        description: description.trim()
      });
      toast('Movimiento registrado.');
      onSaved();
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <Dialog title="Añadir movimiento" onClose={onClose}>
      <div className="seg" style={{ marginBottom: 14 }}>
        <button className={type === 1 ? 'on' : ''} onClick={() => setType(1)}>Ingreso</button>
        <button className={type === 2 ? 'on' : ''} onClick={() => setType(2)}>Egreso</button>
      </div>
      <div className={`field ${amount && !amountOk ? 'bad' : ''}`}>
        <label>Monto</label>
        <input type="number" min="0" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} />
        {amount && !amountOk && <span className="hint">El monto debe ser mayor a 0</span>}
      </div>
      <div className="field">
        <label>Descripción</label>
        <input type="text" maxLength={255} placeholder="Ej. Compra en el supermercado"
          value={description} onChange={(e) => setDescription(e.target.value)} />
      </div>
      {suggestion && (
        <div className="chips" style={{ marginBottom: 14 }}>
          <button className={`chip ${accepted ? 'on' : ''}`} onClick={() => setAccepted(true)}>
            <Icon name={accepted ? 'check' : 'ai'} size={17} />
            IA sugiere: {suggestion.categoryName} ({Math.round(suggestion.confidence * 100)}%)
          </button>
        </div>
      )}
      <div className="field">
        <label>Cuenta origen</label>
        <select value={accountId} onChange={(e) => setAccountId(e.target.value)}>
          {accounts.map((a) => <option key={a.id} value={a.id}>{a.name} ({a.currencyCode})</option>)}
        </select>
      </div>
      {error && <p className="inline-error">{error}</p>}
      <div className="dialog-actions">
        <button className="btn-text" onClick={onClose}>Cancelar</button>
        <button className="btn btn-sm" disabled={!canSave} onClick={save}>{saving ? 'Guardando…' : 'Guardar'}</button>
      </div>
    </Dialog>
  );
}

export default function Transactions() {
  const [open, setOpen] = useState(false);
  const txs = useAsync(() => Api.getTransactions(), []);
  const accounts = useAsync(() => Api.getAccounts(), []);

  const onSaved = () => { setOpen(false); txs.reload(); };

  return (
    <Layout title="Movimientos" actions={
      <button className="btn btn-sm" onClick={() => setOpen(true)} disabled={!accounts.data?.length}>
        <Icon name="add" size={18} /> Añadir
      </button>
    }>
      {txs.error && <ErrorBanner message={txs.error} onRetry={txs.reload} />}
      {txs.loading ? (
        <div className="grid"><Shimmer height={64} /><Shimmer height={64} /><Shimmer height={64} /></div>
      ) : (
        <div className="card">
          {txs.data.length === 0 ? (
            <EmptyState>Aún no registras movimientos. Toca “Añadir” para empezar.</EmptyState>
          ) : (
            <div className="rows">{txs.data.map((t) => <TransactionRow key={t.id} tx={t} />)}</div>
          )}
        </div>
      )}

      {open && accounts.data && (
        <AddTransactionDialog accounts={accounts.data} onClose={() => setOpen(false)} onSaved={onSaved} />
      )}
    </Layout>
  );
}
