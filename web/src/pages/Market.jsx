import { useRef, useState } from 'react';
import Layout from '../components/Layout.jsx';
import { Icon } from '../components/Icon.jsx';
import { EmptyState, ErrorBanner, Shimmer, StaleBadge } from '../components/common.jsx';
import { useAsync } from '../lib/useAsync.js';
import { Api } from '../api/index.js';
import { formatMoney, formatDate, shortDate, formatDelta } from '../lib/format.js';

const CHART = { w: 360, h: 180, left: 46, right: 14, top: 12, bottom: 24 };

function niceTicks(min, max) {
  if (min === max) { min -= 1; max += 1; }
  const rough = (max - min) / 3;
  const mag = Math.pow(10, Math.floor(Math.log10(rough)));
  const step = [1, 2, 2.5, 5, 10].map((m) => m * mag).find((s) => s >= rough);
  const low = Math.floor(min / step) * step;
  const high = Math.ceil(max / step) * step;
  const ticks = [];
  for (let v = low; v <= high + step / 2; v += step) ticks.push(v);
  const decimals = step >= 1 ? (Number.isInteger(step) ? 0 : 1) : 2;
  return { low, high, ticks, decimals };
}

function Sparkline({ history }) {
  if (history.length < 2) return null;
  const closes = history.map((p) => p.close);
  const low = Math.min(...closes), high = Math.max(...closes), span = high - low || 1;
  const step = (84 - 12) / (closes.length - 1);
  const pts = closes.map((v, i) => `${(6 + i * step).toFixed(1)},${(32 - 6 - ((v - low) / span) * 20).toFixed(1)}`);
  const last = pts[pts.length - 1].split(',');
  return (
    <svg className="spark" viewBox="0 0 84 34" aria-hidden="true">
      <polyline points={pts.join(' ')} fill="none" stroke="var(--chart-soft)" strokeWidth="2" strokeLinejoin="round" strokeLinecap="round" />
      <circle cx={last[0]} cy={last[1]} r="3.5" fill="var(--chart)" stroke="var(--surface)" strokeWidth="2" />
    </svg>
  );
}

function PriceChart({ quote }) {
  const [sel, setSel] = useState(-1);
  const svgRef = useRef(null);
  const h = quote.history;
  const closes = h.map((p) => p.close);
  const scale = niceTicks(Math.min(...closes), Math.max(...closes));
  const { w, left, right, top, bottom } = CHART;
  const height = CHART.h;
  const baseline = height - bottom;
  const plotW = w - left - right, plotH = baseline - top;
  const frac = (v) => (v - scale.low) / (scale.high - scale.low);
  const pts = closes.map((v, i) => ({ x: left + (i * plotW) / (closes.length - 1), y: baseline - frac(v) * plotH }));
  const path = pts.map((p) => `${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(' ');
  const last = pts[pts.length - 1];
  const fmtTick = (t) => t.toFixed(scale.decimals);

  const onMove = (e) => {
    const box = svgRef.current.getBoundingClientRect();
    const x = ((e.clientX - box.left) / box.width) * w;
    const step = (last.x - pts[0].x) / (pts.length - 1);
    setSel(Math.max(0, Math.min(pts.length - 1, Math.round((x - pts[0].x) / step))));
  };

  const selP = sel >= 0 ? pts[sel] : null;
  const tipX = selP ? Math.max(18, Math.min(82, (selP.x / w) * 100)) : 0;

  return (
    <div className="chart-wrap">
      <svg ref={svgRef} viewBox={`0 0 ${w} ${height}`} onMouseMove={onMove} onMouseLeave={() => setSel(-1)}
        onTouchMove={(e) => onMove(e.touches[0])}>
        {scale.ticks.map((t, i) => {
          const y = baseline - frac(t) * plotH;
          return <g key={i}>
            <line x1={left} x2={w - right} y1={y} y2={y} stroke="var(--line)" strokeWidth="1" />
            <text x={left - 6} y={y + 3.5} textAnchor="end">{fmtTick(t)}</text>
          </g>;
        })}
        <polygon points={`${pts[0].x},${baseline} ${path} ${last.x},${baseline}`} fill="var(--chart)" fillOpacity="0.1" />
        <polyline points={path} fill="none" stroke="var(--chart)" strokeWidth="2" strokeLinejoin="round" strokeLinecap="round" />
        <circle cx={last.x} cy={last.y} r="4" fill="var(--chart)" stroke="var(--surface)" strokeWidth="2" />
        <text x={pts[0].x} y={height - 6} textAnchor="start">{shortDate(h[0].date)}</text>
        <text x={last.x} y={height - 6} textAnchor="end">{shortDate(h[h.length - 1].date)}</text>
        {selP && <g>
          <line x1={selP.x} x2={selP.x} y1={top} y2={baseline} stroke="var(--chart-soft)" strokeWidth="1" />
          <circle cx={selP.x} cy={selP.y} r="4" fill="var(--chart)" stroke="var(--surface)" strokeWidth="2" />
        </g>}
      </svg>
      {selP && (
        <div className="chart-tip" style={{ left: `${tipX}%`, top: `${(selP.y / height) * 100}%`, transform: `translate(-50%, ${selP.y < height * 0.4 ? '14px' : 'calc(-100% - 14px)'})` }}>
          <b>{formatMoney(h[sel].close, quote.currency)}</b>
          <span>{formatDate(h[sel].date)}</span>
        </div>
      )}
      <details className="chart-table">
        <summary>Ver datos en tabla</summary>
        <table><tbody>
          {h.map((p, i) => <tr key={i}><td>{formatDate(p.date)}</td><td>{formatMoney(p.close, quote.currency)}</td></tr>)}
        </tbody></table>
      </details>
    </div>
  );
}

function StockCard({ quote }) {
  const [open, setOpen] = useState(false);
  const dir = quote.change > 0 ? 'up' : quote.change < 0 ? 'down' : 'flat';
  return (
    <div className="card stock">
      <button className="stock-head" aria-expanded={open} onClick={() => setOpen(!open)}>
        <div className="stock-id"><div className="n">{quote.name}</div><div className="s">{quote.symbol}</div></div>
        <Sparkline history={quote.history} />
        <div className="stock-figs">
          <div className="p tnum">{formatMoney(quote.price, quote.currency)}</div>
          <div className={`d ${dir} tnum`}><Icon name={dir} size={17} />{formatDelta(quote.change, quote.changePercent)}</div>
        </div>
      </button>
      {quote.isStale && <div style={{ padding: '0 18px 12px' }}><StaleBadge>{quote.warningMessage}</StaleBadge></div>}
      {open && <div className="stock-detail"><PriceChart quote={quote} /></div>}
    </div>
  );
}

export default function Market() {
  const rate = useAsync(() => Api.getUsdCopRate(), []);
  const stocks = useAsync(() => Api.getStocks(), []);
  const loading = rate.loading || stocks.loading;

  return (
    <Layout title="Mercado">
      {rate.error && <ErrorBanner message={rate.error} onRetry={() => { rate.reload(); stocks.reload(); }} />}
      {loading ? (
        <div className="grid"><Shimmer height={96} /><Shimmer height={64} /><Shimmer height={64} /></div>
      ) : (
        <>
          {rate.data?.isStale && <StaleBadge>{rate.data.warningMessage}</StaleBadge>}
          {rate.data && (
            <div className="card pad" style={{ marginBottom: 8 }}>
              <div style={{ color: 'var(--text-faint)', fontSize: 12.5, fontWeight: 600 }}>Dólar estadounidense · USD/COP</div>
              <div className="tnum" style={{ fontSize: 30, fontWeight: 800, letterSpacing: '-0.02em', margin: '4px 0' }}>{formatMoney(rate.data.rate, 'COP')}</div>
              <div style={{ color: 'var(--text-faint)', fontSize: 12.5 }}>Actualizado: {formatDate(rate.data.lastUpdated)}</div>
            </div>
          )}
          <div className="section-head"><h2>Acciones</h2></div>
          <p style={{ color: 'var(--text-faint)', fontSize: 13, margin: '-6px 0 12px' }}>Variación frente al cierre anterior. Toca una acción para ver el último mes.</p>
          {!stocks.data?.length ? (
            <div className="card"><EmptyState>Las cotizaciones no están disponibles en este momento. Tus cuentas y movimientos siguen funcionando con normalidad.</EmptyState></div>
          ) : (
            <div className="grid">{stocks.data.map((q) => <StockCard key={q.symbol} quote={q} />)}</div>
          )}
        </>
      )}
    </Layout>
  );
}
