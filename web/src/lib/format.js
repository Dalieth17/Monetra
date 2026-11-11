export function formatMoney(amount, currencyCode = 'COP') {
  const locale = currencyCode === 'USD' ? 'en-US' : 'es-CO';
  const digits = currencyCode === 'USD' ? 2 : 0;
  const formatted = new Intl.NumberFormat(locale, {
    minimumFractionDigits: digits,
    maximumFractionDigits: 2
  }).format(Number(amount));
  return `$${formatted} ${currencyCode}`;
}

export function formatDate(isoString) {
  if (!isoString) return '';
  const date = new Date(String(isoString).substring(0, 19));
  return date.toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function shortDate(isoString) {
  if (!isoString) return '';
  return new Date(String(isoString).substring(0, 19))
    .toLocaleDateString('es-CO', { day: 'numeric', month: 'short' });
}

export function formatDelta(change, changePercent) {
  const c = Number(change);
  const p = Number(changePercent);
  const sign = c > 0 ? '+' : c < 0 ? '−' : '';
  return `${sign}${Math.abs(c).toFixed(2)} (${sign}${Math.abs(p).toFixed(2)}%)`;
}

export function toInputDate(isoString) {
  if (!isoString) return '';
  return String(isoString).substring(0, 10);
}
