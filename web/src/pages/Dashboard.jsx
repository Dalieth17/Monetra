import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/Layout.jsx';
import { Icon } from '../components/Icon.jsx';
import { Shimmer, EmptyState, ErrorBanner, StaleBadge } from '../components/common.jsx';
import { useAsync } from '../lib/useAsync.js';
import { Api } from '../api/index.js';
import { formatMoney, formatDate } from '../lib/format.js';

function CurrencyToggle({ value, onChange }) {
  return (
    <div className="ccy-toggle" role="group" aria-label="Moneda base">
      {['COP', 'USD'].map((c) => (
        <button key={c} className={value === c ? 'on' : ''} onClick={() => onChange(c)}>{c}</button>
      ))}
    </div>
  );
}

export function TransactionRow({ tx }) {
  const income = tx.type === 1;
  return (
    <div className="row-item">
      <div className="row-ico"><Icon name={income ? 'trendUp' : 'receipt'} size={20} /></div>
      <div className="row-main">
        <div className="t">{tx.description || tx.categoryName}</div>
        <div className="s">{tx.categoryName} · {tx.accountName} · {formatDate(tx.date)}</div>
      </div>
      <div className={`amt ${income ? 'in' : 'out'} tnum`}>
        {income ? '+' : '−'}{formatMoney(tx.amount, tx.currencyCode)}
      </div>
    </div>
  );
}

export default function Dashboard() {
  const navigate = useNavigate();
  const [currency, setCurrency] = useState('COP');
  const { data, loading, error, reload } = useAsync(() => Api.getDashboard(currency), [currency]);

  return (
    <Layout title="Panel" actions={<CurrencyToggle value={currency} onChange={setCurrency} />}>
      {error && <ErrorBanner message={error} onRetry={reload} />}

      {loading ? (
        <div className="grid">
          <Shimmer height={150} />
          <div className="accounts-grid"><Shimmer height={96} /><Shimmer height={96} /></div>
          <Shimmer height={64} /><Shimmer height={64} />
        </div>
      ) : data ? (
        <>
          {data.isRateCached && <StaleBadge>Tasa de cambio desactualizada (último valor: {formatDate(data.exchangeRateTimestamp)})</StaleBadge>}

          <div className="hero" style={{ marginTop: data.isRateCached ? 12 : 0 }}>
            <div className="label">Patrimonio neto · {data.baseCurrency}</div>
            <div className="net tnum">{formatMoney(data.totalNetWorth, data.baseCurrency)}</div>
            <div className="trm tnum">TRM USD/COP: {data.usdToCopExchangeRate}</div>
            <div className="flow">
              <div className="flow-item in">
                <div className="dot"><Icon name="up" size={19} /></div>
                <div><small>Ingresos del mes</small><b className="tnum">{formatMoney(data.monthlyIncome, data.baseCurrency)}</b></div>
              </div>
              <div className="flow-item out">
                <div className="dot"><Icon name="down" size={19} /></div>
                <div><small>Egresos del mes</small><b className="tnum">{formatMoney(data.monthlyExpense, data.baseCurrency)}</b></div>
              </div>
            </div>
          </div>

          <div className="section-head">
            <h2>Mis cuentas</h2>
          </div>
          <div className="accounts-grid">
            {data.accounts.map((a) => (
              <div key={a.id} className="card acct">
                <span className="ccy">{a.currencyCode}</span>
                <span className="name">{a.name}</span>
                <span className="bal tnum">{formatMoney(a.currentBalance, a.currencyCode)}</span>
              </div>
            ))}
            <button className="card acct-add" onClick={() => navigate('/cuentas')}>
              <Icon name="add" size={20} /> Nueva cuenta
            </button>
          </div>

          <div className="section-head">
            <h2>Movimientos recientes</h2>
            <button className="btn btn-sm" onClick={() => navigate('/movimientos')}>
              <Icon name="add" size={18} /> Añadir
            </button>
          </div>
          <div className="card">
            {data.recentTransactions.length === 0 ? (
              <EmptyState>No tienes movimientos este mes. Toca “Añadir” para registrar el primero.</EmptyState>
            ) : (
              <div className="rows">
                {data.recentTransactions.map((t) => <TransactionRow key={t.id} tx={t} />)}
              </div>
            )}
          </div>
        </>
      ) : null}
    </Layout>
  );
}
