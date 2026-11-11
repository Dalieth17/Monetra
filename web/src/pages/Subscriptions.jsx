import { useState } from 'react';
import Layout from '../components/Layout.jsx';
import { Icon } from '../components/Icon.jsx';
import { Dialog, EmptyState, ErrorBanner, Shimmer, Switch, parseAmount } from '../components/common.jsx';
import { useAsync } from '../lib/useAsync.js';
import { Api } from '../api/index.js';
import { useToast } from '../components/Toast.jsx';
import { formatMoney, formatDate } from '../lib/format.js';

const CYCLES = [['Weekly', 'Semanal'], ['Monthly', 'Mensual'], ['Annual', 'Anual']];

function CreateDialog({ onClose, onSaved }) {
  const toast = useToast();
  const [name, setName] = useState('');
  const [amount, setAmount] = useState('');
  const [currencyCode, setCurrency] = useState('COP');
  const [billingCycle, setCycle] = useState('Monthly');
  const [date, setDate] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const a = parseAmount(amount);
  const canSave = name.trim() && a > 0 && date && !saving;

  const save = async () => {
    setSaving(true); setError(null);
    try {
      await Api.createSubscription({ name: name.trim(), amount: a, currencyCode, billingCycle, nextPaymentDate: `${date}T00:00:00` });
      toast('Suscripción registrada.'); onSaved();
    } catch (err) { setError(err.message); setSaving(false); }
  };

  return (
    <Dialog title="Nueva suscripción" onClose={onClose}>
      <div className="field"><label>Nombre</label><input placeholder="Ej. Netflix" value={name} onChange={(e) => setName(e.target.value)} /></div>
      <div className="field"><label>Monto</label><input type="number" min="0" value={amount} onChange={(e) => setAmount(e.target.value)} /></div>
      <div className="chips" style={{ marginBottom: 14 }}>
        {['COP', 'USD'].map((c) => <button key={c} className={`chip ${currencyCode === c ? 'on' : ''}`} onClick={() => setCurrency(c)}>{c}</button>)}
      </div>
      <div className="chips" style={{ marginBottom: 14 }}>
        {CYCLES.map(([v, l]) => <button key={v} className={`chip ${billingCycle === v ? 'on' : ''}`} onClick={() => setCycle(v)}>{l}</button>)}
      </div>
      <div className="field"><label>Próxima fecha de cobro</label><input type="date" value={date} onChange={(e) => setDate(e.target.value)} /></div>
      {error && <p className="inline-error">{error}</p>}
      <div className="dialog-actions">
        <button className="btn-text" onClick={onClose}>Cancelar</button>
        <button className="btn btn-sm" disabled={!canSave} onClick={save}>{saving ? 'Guardando…' : 'Guardar'}</button>
      </div>
    </Dialog>
  );
}

export default function Subscriptions() {
  const toast = useToast();
  const [creating, setCreating] = useState(false);
  const subs = useAsync(() => Api.getSubscriptions(), []);

  const toggle = async (id) => {
    try { await Api.toggleSubscription(id); subs.reload(); } catch (e) { toast(e.message); subs.reload(); }
  };
  const remove = async (id) => {
    try { await Api.deleteSubscription(id); toast('Suscripción eliminada.'); subs.reload(); } catch (e) { toast(e.message); }
  };

  return (
    <Layout title="Suscripciones" actions={
      <button className="btn btn-sm" onClick={() => setCreating(true)}><Icon name="add" size={18} /> Añadir</button>
    }>
      {subs.error && <ErrorBanner message={subs.error} onRetry={subs.reload} />}
      {subs.loading ? (
        <div className="grid"><Shimmer height={92} /><Shimmer height={92} /></div>
      ) : subs.data.length === 0 ? (
        <div className="card"><EmptyState>No tienes suscripciones registradas. Mapea tus gastos fijos con “Añadir”.</EmptyState></div>
      ) : (
        <div className="grid">
          {subs.data.map((s) => (
            <div key={s.id} className="card pad">
              <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 700, fontSize: 17 }}>{s.name}</div>
                  <div style={{ color: 'var(--text-faint)', fontSize: 12.5 }}>{s.billingCycle} · Próximo cobro: {formatDate(s.nextPaymentDate)}</div>
                </div>
                <Switch checked={s.isActive} onChange={() => toggle(s.id)} />
                <button className="icon-btn danger" onClick={() => remove(s.id)} aria-label="Eliminar"><Icon name="del" size={20} /></button>
              </div>
              <div className="tnum" style={{ fontSize: 15, marginTop: 8 }}>{formatMoney(s.amount, s.currencyCode)}</div>
              {s.isActive && s.daysUntilPayment <= 3 && (
                <div className="warn-row"><Icon name="bell" size={16} />
                  {s.daysUntilPayment === 0 ? '¡El cobro es hoy!' : `Cobro en ${s.daysUntilPayment} día(s)`}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {creating && <CreateDialog onClose={() => setCreating(false)} onSaved={() => { setCreating(false); subs.reload(); }} />}
    </Layout>
  );
}
