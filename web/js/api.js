/* Cliente de la API Monetra (.NET 8) - espejo de MonetraApi.kt */

// Por defecto se usa el mismo origen (/api), que nginx redirige a la API en Docker.
// Para desarrollo fuera de Docker: localStorage.setItem('monetra_api_url', 'http://localhost:5127').
const API_BASE_URL = localStorage.getItem('monetra_api_url') || '';

const Session = {
    get token() { return localStorage.getItem('monetra_token'); },
    get userName() { return localStorage.getItem('monetra_name') || ''; },
    isLoggedIn() { return !!this.token; },
    save(auth) {
        localStorage.setItem('monetra_token', auth.token);
        localStorage.setItem('monetra_name', auth.name);
        localStorage.setItem('monetra_email', auth.email);
    },
    saveLastRate(rate, timestamp) {
        localStorage.setItem('monetra_last_rate', String(rate));
        localStorage.setItem('monetra_last_rate_at', timestamp);
    },
    get lastRate() { return parseFloat(localStorage.getItem('monetra_last_rate') || '0'); },
    get lastRateAt() { return localStorage.getItem('monetra_last_rate_at') || ''; },
    get isDemo() { return localStorage.getItem('monetra_demo') === '1'; },
    enterDemo() {
        const auth = DemoApi.enter();
        localStorage.setItem('monetra_demo', '1');
        this.save(auth);
    },
    clear() {
        ['monetra_token', 'monetra_name', 'monetra_email', 'monetra_demo'].forEach(k => localStorage.removeItem(k));
    }
};

class OfflineError extends Error {
    constructor() { super('Sin conexión con el servidor.'); this.offline = true; }
}

async function apiCall(method, path, body = null) {
    const headers = { 'Content-Type': 'application/json' };
    if (Session.token && !path.startsWith('/api/auth/')) {
        headers['Authorization'] = `Bearer ${Session.token}`;
    }

    let response;
    try {
        response = await fetch(API_BASE_URL + path, {
            method,
            headers,
            body: body ? JSON.stringify(body) : null
        });
    } catch (_) {
        throw new OfflineError();
    }

    let json = null;
    try { json = await response.json(); } catch (_) { /* cuerpo vacío */ }

    if (!response.ok || !json || !json.success) {
        const message = json?.message
            || (json?.errors && Object.values(json.errors).flat()[0])
            || defaultHttpMessage(response.status);
        const error = new Error(message);
        error.status = response.status;
        throw error;
    }
    return json.data;
}

function defaultHttpMessage(code) {
    switch (code) {
        case 401: return 'Sesión expirada. Inicia sesión nuevamente.';
        case 403: return 'No tienes permisos para esta operación.';
        case 404: return 'Recurso no encontrado.';
        default: return `Error del servidor (${code}). Intenta de nuevo.`;
    }
}

const RealApi = {
    // Auth
    register: (name, email, password) => apiCall('POST', '/api/auth/register', { name, email, password }),
    login: (email, password) => apiCall('POST', '/api/auth/login', { email, password }),

    // Dashboard
    getDashboard: (baseCurrency = 'COP') => apiCall('GET', `/api/dashboard?baseCurrency=${baseCurrency}`),

    // Transactions
    createTransaction: (tx) => apiCall('POST', '/api/transactions', tx),

    // AI
    suggestCategory: (description) =>
        apiCall('GET', `/api/ai/suggest-category?description=${encodeURIComponent(description)}`),

    // Market
    getUsdCopRate: () => apiCall('GET', '/api/market/usd-cop'),
    getStocks: () => apiCall('GET', '/api/market/stocks'),

    // Debts
    getDebts: () => apiCall('GET', '/api/debts'),
    createDebt: (debt) => apiCall('POST', '/api/debts', debt),
    recordDebtPayment: (id, payment) => apiCall('POST', `/api/debts/${id}/payments`, payment),
    deleteDebt: (id) => apiCall('DELETE', `/api/debts/${id}`),

    // Savings Goals
    getSavingsGoals: () => apiCall('GET', '/api/savingsgoals'),
    createSavingsGoal: (goal) => apiCall('POST', '/api/savingsgoals', goal),
    depositToGoal: (id, deposit) => apiCall('POST', `/api/savingsgoals/${id}/deposit`, deposit),
    deleteSavingsGoal: (id) => apiCall('DELETE', `/api/savingsgoals/${id}`),

    // Subscriptions
    getSubscriptions: () => apiCall('GET', '/api/subscriptions'),
    createSubscription: (sub) => apiCall('POST', '/api/subscriptions', sub),
    toggleSubscription: (id) => apiCall('PATCH', `/api/subscriptions/${id}/toggle`),
    deleteSubscription: (id) => apiCall('DELETE', `/api/subscriptions/${id}`)
};

// En modo demostración cada llamada se resuelve con DemoApi (datos en memoria),
// sin tocar la red; en modo normal va al backend real. Los sitios de llamada
// (Api.getDashboard, etc.) no cambian.
const Api = new Proxy({}, {
    get(_, prop) {
        if (Session.isDemo && typeof DemoApi[prop] === 'function') {
            return DemoApi[prop].bind(DemoApi);
        }
        return RealApi[prop];
    }
});
