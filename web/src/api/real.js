import { Session } from './session.js';

// Por defecto mismo origen (/api), que nginx redirige a la API en Docker.
// Para desarrollo fuera de Docker: localStorage.setItem('monetra_api_url', 'http://localhost:5127').
const BASE_URL = localStorage.getItem('monetra_api_url') || '';

export class OfflineError extends Error {
  constructor() {
    super('Sin conexión con el servidor.');
    this.offline = true;
  }
}

function httpMessage(code) {
  switch (code) {
    case 401: return 'Sesión expirada. Inicia sesión nuevamente.';
    case 403: return 'No tienes permisos para esta operación.';
    case 404: return 'Recurso no encontrado.';
    default: return `Error del servidor (${code}). Intenta de nuevo.`;
  }
}

async function call(method, path, body = null) {
  const headers = { 'Content-Type': 'application/json' };
  if (Session.token && !path.startsWith('/api/auth/')) {
    headers.Authorization = `Bearer ${Session.token}`;
  }

  let response;
  try {
    response = await fetch(BASE_URL + path, {
      method,
      headers,
      body: body ? JSON.stringify(body) : null
    });
  } catch {
    throw new OfflineError();
  }

  let json = null;
  try { json = await response.json(); } catch { /* cuerpo vacío */ }

  if (!response.ok || !json || !json.success) {
    const message =
      json?.message ||
      (json?.errors && Object.values(json.errors).flat()[0]) ||
      httpMessage(response.status);
    const error = new Error(message);
    error.status = response.status;
    throw error;
  }
  return json.data;
}

export const RealApi = {
  register: (name, email, password) => call('POST', '/api/auth/register', { name, email, password }),
  login: (email, password) => call('POST', '/api/auth/login', { email, password }),

  getDashboard: (baseCurrency = 'COP') => call('GET', `/api/dashboard?baseCurrency=${baseCurrency}`),

  getAccounts: () => call('GET', '/api/accounts'),
  createAccount: (account) => call('POST', '/api/accounts', account),

  getTransactions: () => call('GET', '/api/transactions'),
  createTransaction: (tx) => call('POST', '/api/transactions', tx),
  deleteTransaction: (id) => call('DELETE', `/api/transactions/${id}`),

  suggestCategory: (description) =>
    call('GET', `/api/ai/suggest-category?description=${encodeURIComponent(description)}`),

  getUsdCopRate: () => call('GET', '/api/market/usd-cop'),
  getStocks: () => call('GET', '/api/market/stocks'),

  getDebts: () => call('GET', '/api/debts'),
  createDebt: (debt) => call('POST', '/api/debts', debt),
  recordDebtPayment: (id, payment) => call('POST', `/api/debts/${id}/payments`, payment),
  deleteDebt: (id) => call('DELETE', `/api/debts/${id}`),

  getSavingsGoals: () => call('GET', '/api/savingsgoals'),
  createSavingsGoal: (goal) => call('POST', '/api/savingsgoals', goal),
  depositToGoal: (id, deposit) => call('POST', `/api/savingsgoals/${id}/deposit`, deposit),
  deleteSavingsGoal: (id) => call('DELETE', `/api/savingsgoals/${id}`),

  getSubscriptions: () => call('GET', '/api/subscriptions'),
  createSubscription: (sub) => call('POST', '/api/subscriptions', sub),
  toggleSubscription: (id) => call('PATCH', `/api/subscriptions/${id}/toggle`),
  deleteSubscription: (id) => call('DELETE', `/api/subscriptions/${id}`)
};
