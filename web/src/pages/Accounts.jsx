import { useState } from 'react';
import Layout from '../components/Layout.jsx';
import { Icon } from '../components/Icon.jsx';
import { Dialog, EmptyState, ErrorBanner, Shimmer, parseAmount } from '../components/common.jsx';
import { useAsync } from '../lib/useAsync.js';
import { Api } from '../api/index.js';
import { useToast } from '../components/Toast.jsx';
import { formatMoney } from '../lib/format.js';

const CURRENCIES = ['COP', 'USD', 'EUR'];

function CreateAccountDialog({ onClose, onSaved }) {
  const toast = useToast();
  const [name, setName] = useState('');
  const [currencyCode, setCurrency] = useState('COP');
  const [balance, setBalance] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  const initial = parseAmount(balance) ?? 0;
  const canSave = name.trim() && initial >= 0 && !saving;

  const save = async () => {
    setSaving(true);
    setError(null);
    try {
      await Api.createAccount({ name: name.trim(), currencyCode, initialBalance: initial });
      toast('Cuenta creada.');
      onSaved();
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <Dialog title="Nueva cuenta" onClose={onClose}>
      <div className="field">
        <label>Nombre</label>
        <input type="text" placeholder="Ej. Ahorros Bancolombia" value={name} onChange={(e) => setName(e.target.value)} />
      </div>
      <div className="field">
        <label>Divisa</label>
        <select value={currencyCode} onChange={(e) => setCurrency(e.target.value)}>
          {CURRENCIES.map((c) => <option key={c} value={c}>{c}</option>)}
        </select>
      </div>
      <div className="field">
        <label>Saldo inicial (opcional)</label>
        <input type="number" min="0" step="0.01" value={balance} onChange={(e) => setBalance(e.target.value)} />
      </div>
      {error && <p className="inline-error">{error}</p>}
      <div className="dialog-actions">
        <button className="btn-text" onClick={onClose}>Cancelar</button>
        <button className="btn btn-sm" disabled={!canSave} onClick={save}>{saving ? 'Guardando…' : 'Crear'}</button>
      </div>
    </Dialog>
  );
}

export default function Accounts() {
  const [open, setOpen] = useState(false);
  const { data, loading, error, reload } = useAsync(() => Api.getAccounts(), []);

  const onSaved = () => { setOpen(false); reload(); };

  return (
    <Layout title="Cuentas" actions={
      <button className="btn btn-sm" onClick={() => setOpen(true)}><Icon name="add" size={18} /> Nueva</button>
    }>
      {error && <ErrorBanner message={error} onRetry={reload} />}
      {loading ? (
        <div className="accounts-grid"><Shimmer height={96} /><Shimmer height={96} /></div>
      ) : data.length === 0 ? (
        <div className="card"><EmptyState>No tienes cuentas. Crea una con “Nueva”.</EmptyState></div>
      ) : (
        <div className="accounts-grid">
          {data.map((a) => (
            <div key={a.id} className="card acct">
              <span className="ccy">{a.currencyCode}</span>
              <span className="name">{a.name}</span>
              <span className="bal tnum">{formatMoney(a.currentBalance, a.currencyCode)}</span>
            </div>
          ))}
          <button className="card acct-add" onClick={() => setOpen(true)}>
            <Icon name="add" size={20} /> Nueva cuenta
          </button>
        </div>
      )}

      {open && <CreateAccountDialog onClose={() => setOpen(false)} onSaved={onSaved} />}
    </Layout>
  );
}
