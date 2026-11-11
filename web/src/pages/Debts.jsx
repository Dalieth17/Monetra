import { useState } from 'react';
import Layout from '../components/Layout.jsx';
import { Icon } from '../components/Icon.jsx';
import { Dialog, EmptyState, ErrorBanner, Shimmer, parseAmount } from '../components/common.jsx';
import { useAsync } from '../lib/useAsync.js';
import { Api } from '../api/index.js';
import { useToast } from '../components/Toast.jsx';
import { formatMoney, formatDate, toInputDate } from '../lib/format.js';

function CreateDebtDialog({ onClose, onSaved }) {
  const toast = useToast();
  const [creditor, setCreditor] = useState('');
  const [amount, setAmount] = useState('');
  const [rate, setRate] = useState('');
  const [rateType, setRateType] = useState('EA');
  const [due, setDue] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  const a = parseAmount(amount);
  const r = parseAmount(rate);
  const canSave = creditor.trim() && a > 0 && r >= 0 && due && !saving;

  const save = async () => {
    setSaving(true); setError(null);
    try {
      await Api.createDebt({ creditor: creditor.trim(), totalAmount: a, interestRate: r, rateType, dueDate: `${due}T00:00:00` });
      toast('Deuda registrada.'); onSaved();
    } catch (err) { setError(err.message); setSaving(false); }
  };

  return (
    <Dialog title="Registrar deuda" onClose={onClose}>
      <div className="field"><label>Entidad acreedora</label><input value={creditor} onChange={(e) => setCreditor(e.target.value)} /></div>
      <div className="field"><label>Capital inicial</label><input type="number" min="0" value={amount} onChange={(e) => setAmount(e.target.value)} /></div>
      <div className="field"><label>Tasa de interés (%)</label><input type="number" min="0" step="0.01" value={rate} onChange={(e) => setRate(e.target.value)} /></div>
      <div className="chips" style={{ marginBottom: 14 }}>
        <button className={`chip ${rateType === 'EA' ? 'on' : ''}`} onClick={() => setRateType('EA')}>EA (anual)</button>
        <button className={`chip ${rateType === 'EM' ? 'on' : ''}`} onClick={() => setRateType('EM')}>EM (mensual)</button>
      </div>
      <div className="field"><label>Fecha de vencimiento</label><input type="date" value={due} onChange={(e) => setDue(e.target.value)} /></div>
      {error && <p className="inline-error">{error}</p>}
      <div className="dialog-actions">
        <button className="btn-text" onClick={onClose}>Cancelar</button>
        <button className="btn btn-sm" disabled={!canSave} onClick={save}>{saving ? 'Guardando…' : 'Guardar'}</button>
      </div>
    </Dialog>
  );
}

function PayDialog({ debt, accounts, onClose, onSaved }) {
  const toast = useToast();
  const [amount, setAmount] = useState('');
  const [accountId, setAccountId] = useState(accounts[0]?.id ?? '');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const a = parseAmount(amount);
  const canSave = a > 0 && accountId && !saving;

  const save = async () => {
    setSaving(true); setError(null);
    try {
      await Api.recordDebtPayment(debt.id, { accountId: Number(accountId), amount: a });
      toast('Abono aplicado.'); onSaved();
    } catch (err) { setError(err.message); setSaving(false); }
  };

  return (
    <Dialog title={`Abonar a ${debt.creditor}`} onClose={onClose}>
      <div className="field"><label>Monto del abono</label><input type="number" min="0" value={amount} onChange={(e) => setAmount(e.target.value)} /></div>
      <div className="field"><label>Cuenta origen</label>
        <select value={accountId} onChange={(e) => setAccountId(e.target.value)}>
          {accounts.map((x) => <option key={x.id} value={x.id}>{x.name} ({x.currencyCode})</option>)}
        </select>
      </div>
      {error && <p className="inline-error">{error}</p>}
      <div className="dialog-actions">
        <button className="btn-text" onClick={onClose}>Cancelar</button>
        <button className="btn btn-sm" disabled={!canSave} onClick={save}>{saving ? 'Abonando…' : 'Abonar'}</button>
      </div>
    </Dialog>
  );
}

export default function Debts() {
  const toast = useToast();
  const [creating, setCreating] = useState(false);
  const [paying, setPaying] = useState(null);
  const debts = useAsync(() => Api.getDebts(), []);
  const accounts = useAsync(() => Api.getAccounts(), []);

  const remove = async (id) => {
    try { await Api.deleteDebt(id); toast('Deuda eliminada.'); debts.reload(); }
    catch (e) { toast(e.message); }
  };

  return (
    <Layout title="Deudas" actions={
      <button className="btn btn-sm" onClick={() => setCreating(true)}><Icon name="add" size={18} /> Registrar</button>
    }>
      {debts.error && <ErrorBanner message={debts.error} onRetry={debts.reload} />}
      {debts.loading ? (
        <div className="grid"><Shimmer height={120} /><Shimmer height={120} /></div>
      ) : debts.data.length === 0 ? (
        <div className="card"><EmptyState>No tienes deudas registradas. ¡Excelente salud financiera!</EmptyState></div>
      ) : (
        <div className="grid">
          {debts.data.map((d) => {
            const progress = d.totalAmount > 0 ? Math.min(100, Math.max(0, (1 - d.remainingAmount / d.totalAmount) * 100)) : 0;
            return (
              <div key={d.id} className="card pad">
                <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: 700, fontSize: 17 }}>{d.creditor}</div>
                    <div className="s" style={{ color: 'var(--text-faint)', fontSize: 12.5 }}>Tasa {d.interestRate}% {d.rateType} · Vence {formatDate(d.dueDate)}</div>
                  </div>
                  <button className="icon-btn" onClick={() => setPaying(d)} aria-label="Abonar"><Icon name="pay" size={20} /></button>
                  <button className="icon-btn danger" onClick={() => remove(d.id)} aria-label="Eliminar"><Icon name="del" size={20} /></button>
                </div>
                <div className="track"><div style={{ width: `${progress}%` }} /></div>
                <div className="tnum" style={{ fontSize: 15 }}>Saldo: {formatMoney(d.remainingAmount, 'COP')} de {formatMoney(d.totalAmount, 'COP')}</div>
                <div className="tnum" style={{ color: 'var(--text-faint)', fontSize: 12.5 }}>Cuota mensual estimada: {formatMoney(d.estimatedMonthlyPayment, 'COP')}</div>
              </div>
            );
          })}
        </div>
      )}

      {creating && <CreateDebtDialog onClose={() => setCreating(false)} onSaved={() => { setCreating(false); debts.reload(); }} />}
      {paying && accounts.data && <PayDialog debt={paying} accounts={accounts.data} onClose={() => setPaying(null)} onSaved={() => { setPaying(null); debts.reload(); }} />}
    </Layout>
  );
}
