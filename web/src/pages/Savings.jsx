import { useState } from 'react';
import Layout from '../components/Layout.jsx';
import { Icon } from '../components/Icon.jsx';
import { Dialog, EmptyState, ErrorBanner, Shimmer, parseAmount } from '../components/common.jsx';
import { useAsync } from '../lib/useAsync.js';
import { Api } from '../api/index.js';
import { useToast } from '../components/Toast.jsx';
import { formatMoney, formatDate } from '../lib/format.js';

function CreateGoalDialog({ onClose, onSaved }) {
  const toast = useToast();
  const [name, setName] = useState('');
  const [target, setTarget] = useState('');
  const [deadline, setDeadline] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const t = parseAmount(target);
  const canSave = name.trim() && t > 0 && deadline && !saving;

  const save = async () => {
    setSaving(true); setError(null);
    try {
      await Api.createSavingsGoal({ name: name.trim(), targetAmount: t, deadline: `${deadline}T00:00:00` });
      toast('Meta creada.'); onSaved();
    } catch (err) { setError(err.message); setSaving(false); }
  };

  return (
    <Dialog title="Nueva meta de ahorro" onClose={onClose}>
      <div className="field"><label>Nombre</label><input placeholder="Ej. Viaje a San Andrés" value={name} onChange={(e) => setName(e.target.value)} /></div>
      <div className="field"><label>Monto objetivo</label><input type="number" min="0" value={target} onChange={(e) => setTarget(e.target.value)} /></div>
      <div className="field"><label>Fecha límite</label><input type="date" value={deadline} onChange={(e) => setDeadline(e.target.value)} /></div>
      {error && <p className="inline-error">{error}</p>}
      <div className="dialog-actions">
        <button className="btn-text" onClick={onClose}>Cancelar</button>
        <button className="btn btn-sm" disabled={!canSave} onClick={save}>{saving ? 'Creando…' : 'Crear'}</button>
      </div>
    </Dialog>
  );
}

function DepositDialog({ goal, accounts, onClose, onSaved }) {
  const [amount, setAmount] = useState('');
  const [accountId, setAccountId] = useState(accounts[0]?.id ?? '');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const a = parseAmount(amount);
  const canSave = a > 0 && accountId && !saving;

  const save = async () => {
    setSaving(true); setError(null);
    try {
      const updated = await Api.depositToGoal(goal.id, { accountId: Number(accountId), amount: a });
      onSaved(updated);
    } catch (err) { setError(err.message); setSaving(false); }
  };

  return (
    <Dialog title={`Depositar en "${goal.name}"`} onClose={onClose}>
      <div className="field"><label>Monto a depositar</label><input type="number" min="0" value={amount} onChange={(e) => setAmount(e.target.value)} /></div>
      <div className="field"><label>Cuenta origen</label>
        <select value={accountId} onChange={(e) => setAccountId(e.target.value)}>
          {accounts.map((x) => <option key={x.id} value={x.id}>{x.name} ({x.currencyCode})</option>)}
        </select>
      </div>
      {error && <p className="inline-error">{error}</p>}
      <div className="dialog-actions">
        <button className="btn-text" onClick={onClose}>Cancelar</button>
        <button className="btn btn-sm" disabled={!canSave} onClick={save}>{saving ? 'Depositando…' : 'Depositar'}</button>
      </div>
    </Dialog>
  );
}

export default function Savings() {
  const toast = useToast();
  const [creating, setCreating] = useState(false);
  const [depositing, setDepositing] = useState(null);
  const [celebrate, setCelebrate] = useState(null);
  const goals = useAsync(() => Api.getSavingsGoals(), []);
  const accounts = useAsync(() => Api.getAccounts(), []);

  const remove = async (id) => {
    try { await Api.deleteSavingsGoal(id); toast('Meta eliminada.'); goals.reload(); }
    catch (e) { toast(e.message); }
  };

  const afterDeposit = (updated) => {
    setDepositing(null);
    goals.reload();
    if (updated.isCompleted) setCelebrate(updated.name);
    else toast('Depósito registrado.');
  };

  return (
    <Layout title="Metas de ahorro" actions={
      <button className="btn btn-sm" onClick={() => setCreating(true)}><Icon name="add" size={18} /> Crear</button>
    }>
      {goals.error && <ErrorBanner message={goals.error} onRetry={goals.reload} />}
      {goals.loading ? (
        <div className="grid"><Shimmer height={120} /><Shimmer height={120} /></div>
      ) : goals.data.length === 0 ? (
        <div className="card"><EmptyState>No tienes metas de ahorro activas. Crea una con “Crear”.</EmptyState></div>
      ) : (
        <div className="grid">
          {goals.data.map((g) => (
            <div key={g.id} className="card pad">
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 700, fontSize: 17 }}>{g.name}</div>
                  <div style={{ color: 'var(--text-faint)', fontSize: 12.5 }}>Fecha límite: {formatDate(g.deadline)}</div>
                </div>
                <button className="icon-btn" onClick={() => setDepositing(g)} aria-label="Depositar"><Icon name="savings" size={20} /></button>
                <button className="icon-btn danger" onClick={() => remove(g.id)} aria-label="Eliminar"><Icon name="del" size={20} /></button>
              </div>
              <div className="track"><div style={{ width: `${Math.min(100, g.progressPercentage)}%` }} /></div>
              <div className="tnum" style={{ fontSize: 15 }}>{formatMoney(g.currentAmount, 'COP')} de {formatMoney(g.targetAmount, 'COP')} ({Math.floor(g.progressPercentage)}%)</div>
            </div>
          ))}
        </div>
      )}

      {creating && <CreateGoalDialog onClose={() => setCreating(false)} onSaved={() => { setCreating(false); goals.reload(); }} />}
      {depositing && accounts.data && <DepositDialog goal={depositing} accounts={accounts.data} onClose={() => setDepositing(null)} onSaved={afterDeposit} />}
      {celebrate && (
        <Dialog onClose={() => setCelebrate(null)}>
          <div style={{ textAlign: 'center' }}>
            <Icon name="star" size={48} className="" />
            <h2 style={{ marginTop: 8 }}>¡Meta completada!</h2>
            <p style={{ color: 'var(--text-soft)', marginBottom: 16 }}>Felicitaciones, alcanzaste tu meta “{celebrate}”. Tu disciplina está dando frutos.</p>
            <button className="btn full" onClick={() => setCelebrate(null)}>Ver progreso</button>
          </div>
        </Dialog>
      )}
    </Layout>
  );
}
