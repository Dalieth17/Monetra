import { useEffect } from 'react';
import { Icon } from './Icon.jsx';

export function Shimmer({ height = 80, style }) {
  return <div className="shimmer" style={{ height, ...style }} />;
}

export function EmptyState({ children }) {
  return (
    <div className="empty">
      <Icon name="receipt" size={54} />
      <p>{children}</p>
    </div>
  );
}

export function ErrorBanner({ message, onRetry }) {
  return (
    <div className="error-banner">
      <span>{message}</span>
      {onRetry && <button onClick={onRetry}>Reintentar</button>}
    </div>
  );
}

export function StaleBadge({ children }) {
  return (
    <div className="badge-stale">
      <Icon name="cloudOff" size={18} />
      <span>{children}</span>
    </div>
  );
}

export function Switch({ checked, onChange }) {
  return (
    <label className="switch">
      <input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} />
      <span className="tr"><span className="th" /></span>
    </label>
  );
}

export function Dialog({ title, onClose, children }) {
  useEffect(() => {
    const onKey = (e) => e.key === 'Escape' && onClose();
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  return (
    <div className="overlay" onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <div className="dialog" role="dialog" aria-modal="true" aria-label={title}>
        {title && <h2>{title}</h2>}
        {children}
      </div>
    </div>
  );
}

/** Parsea texto a número decimal (acepta coma). */
export function parseAmount(text) {
  if (!text || !String(text).trim()) return null;
  const n = Number(String(text).replace(',', '.'));
  return Number.isFinite(n) ? n : null;
}
