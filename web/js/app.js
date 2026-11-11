/* Monetra Web - SPA que replica las pantallas Compose de la app Android */

// ---------- Iconos Material (inline SVG) ----------
const Icons = {
    wallet: '<path d="M21 18v1c0 1.1-.9 2-2 2H5c-1.11 0-2-.9-2-2V5c0-1.1.89-2 2-2h14c1.1 0 2 .9 2 2v1h-9c-1.11 0-2 .9-2 2v8c0 1.1.89 2 2 2h9zm-9-2h10V8H12v8zm4-2.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5z"/>',
    add: '<path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z"/>',
    back: '<path d="M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z"/>',
    logout: '<path d="M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z"/>',
    swap: '<path d="M6.99 11L3 15l3.99 4v-3H14v-2H6.99v-3zM21 9l-3.99-4v3H10v2h7.01v3L21 9z"/>',
    trendingUp: '<path d="M16 6l2.29 2.29-4.88 4.88-4-4L2 16.59 3.41 18l6-6 4 4 6.3-6.29L22 12V6z"/>',
    trendingDown: '<path d="M16 18l2.29-2.29-4.88-4.88-4 4L2 7.41 3.41 6l6 6 4-4 6.3 6.29L22 12v6z"/>',
    creditCard: '<path d="M20 4H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4v-6h16v6zm0-10H4V6h16v2z"/>',
    savings: '<path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1.41 16.09V20h-2.67v-1.93c-1.71-.36-3.16-1.46-3.27-3.4h1.96c.1 1.05.82 1.87 2.65 1.87 1.96 0 2.4-.98 2.4-1.59 0-.83-.44-1.61-2.67-2.14-2.48-.6-4.18-1.62-4.18-3.67 0-1.72 1.39-2.84 3.11-3.21V4h2.67v1.95c1.86.45 2.79 1.86 2.85 3.39H14.3c-.05-1.11-.64-1.87-2.22-1.87-1.5 0-2.4.68-2.4 1.64 0 .84.65 1.39 2.67 1.91s4.18 1.39 4.18 3.91c-.01 1.83-1.38 2.83-3.12 3.16z"/>',
    subscriptions: '<path d="M20 8H4V6h16v2zm-2-6H6v2h12V2zm4 10v8c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2v-8c0-1.1.9-2 2-2h16c1.1 0 2 .9 2 2zm-6 4l-6-3.27v6.53L16 16z"/>',
    cloudOff: '<path d="M19.35 10.04C18.67 6.59 15.64 4 12 4c-1.48 0-2.85.43-4.01 1.17l1.46 1.46C10.21 6.23 11.08 6 12 6c3.04 0 5.5 2.46 5.5 5.5v.5H19c1.66 0 3 1.34 3 3 0 1.13-.64 2.11-1.56 2.62l1.45 1.45C23.16 18.16 24 16.68 24 15c0-2.64-2.05-4.78-4.65-4.96zM3 5.27l2.75 2.74C2.56 8.15 0 10.77 0 14c0 3.31 2.69 6 6 6h11.73l2 2L21 20.73 4.27 4 3 5.27zM7.73 10l8 8H6c-2.21 0-4-1.79-4-4s1.79-4 4-4h1.73z"/>',
    del: '<path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"/>',
    payments: '<path d="M19 14V6c0-1.1-.9-2-2-2H3c-1.1 0-2 .9-2 2v8c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zm-9-1c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3zm13-6v11c0 1.1-.9 2-2 2H4v-2h17V7h2z"/>',
    star: '<path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/>',
    bell: '<path d="M18 11c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2v-5zm-6 11c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2z"/>',
    receipt: '<path d="M19.5 3.5L18 2l-1.5 1.5L15 2l-1.5 1.5L12 2l-1.5 1.5L9 2 7.5 3.5 6 2v16H3v3c0 1.66 1.34 3 3 3h12c1.66 0 3-1.34 3-3V2l-1.5 1.5zM15 20H6c-.55 0-1-.45-1-1v-1h10v2zm4-1c0 .55-.45 1-1 1s-1-.45-1-1v-3H8V5h11v14zM9 7h6v2H9zm7 0h2v2h-2zm-7 4h6v2H9zm7 0h2v2h-2z"/>',
    ai: '<path d="M19 9l1.25-2.75L23 5l-2.75-1.25L19 1l-1.25 2.75L15 5l2.75 1.25L19 9zm-7.5.5L9 4 6.5 9.5 1 12l5.5 2.5L9 20l2.5-5.5L17 12l-5.5-2.5zM19 15l-1.25 2.75L15 19l2.75 1.25L19 23l1.25-2.75L23 19l-2.75-1.25L19 15z"/>',
    check: '<path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/>'
};
const icon = (name, size = 24) =>
    `<svg viewBox="0 0 24 24" width="${size}" height="${size}" fill="currentColor">${Icons[name]}</svg>`;

// ---------- Utilidades ----------
function formatMoney(amount, currencyCode) {
    const locale = currencyCode === 'USD' ? 'en-US' : 'es-CO';
    const digits = currencyCode === 'USD' ? 2 : 0;
    const formatted = new Intl.NumberFormat(locale, {
        minimumFractionDigits: digits, maximumFractionDigits: 2
    }).format(amount);
    return `$${formatted} ${currencyCode}`;
}

function formatDate(isoString) {
    if (!isoString) return '';
    const date = new Date(isoString.substring(0, 19));
    return date.toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });
}

function showSnackbar(message) {
    const bar = document.getElementById('snackbar');
    bar.textContent = message;
    bar.classList.remove('hidden');
    clearTimeout(bar._timer);
    bar._timer = setTimeout(() => bar.classList.add('hidden'), 3500);
}

function esc(text) {
    const div = document.createElement('div');
    div.textContent = text ?? '';
    return div.innerHTML;
}

const $ = (selector) => document.querySelector(selector);
const app = () => document.getElementById('app');

// ---------- Estado global ----------
const State = {
    baseCurrency: 'COP',
    dashboard: null
};

// Las cuentas de los diálogos (abonar/depositar) salen del dashboard cacheado.
// Al entrar directo a /debts o /savings (deep link), aún no está, así que se carga.
async function ensureAccounts() {
    if (!State.dashboard) {
        try { State.dashboard = await Api.getDashboard(State.baseCurrency); } catch (_) { /* las vistas muestran su propio error */ }
    }
}

// ---------- Router ----------
const routes = {
    '#/login': renderLogin,
    '#/register': renderRegister,
    '#/dashboard': renderDashboard,
    '#/add': renderAddTransaction,
    '#/debts': renderDebts,
    '#/savings': renderSavings,
    '#/subscriptions': renderSubscriptions
};

function navigate(hash) { location.hash = hash; }

function render() {
    const hash = location.hash || (Session.isLoggedIn() ? '#/dashboard' : '#/login');
    if (!Session.isLoggedIn() && hash !== '#/login' && hash !== '#/register') {
        navigate('#/login');
        return;
    }
    (routes[hash] || renderLogin)();
}

window.addEventListener('hashchange', render);
window.addEventListener('DOMContentLoaded', render);

// ---------- Vistas: Autenticación ----------
function renderLogin() {
    app().innerHTML = `
        <div class="auth-container">
            <svg class="auth-logo" viewBox="0 0 24 24" fill="#00A86B">${Icons.wallet}</svg>
            <h1>Bienvenido a Monetra</h1>
            <p class="auth-subtitle">Toma el control soberano de tu dinero</p>
            <div class="field" id="field-email">
                <label>Correo electrónico</label>
                <input type="email" id="email" autocomplete="email">
                <span class="support hidden" id="email-error">Formato de correo inválido</span>
            </div>
            <div class="field">
                <label>Contraseña</label>
                <input type="password" id="password" autocomplete="current-password">
            </div>
            <p class="inline-error hidden" id="form-error"></p>
            <button class="btn-primary" id="login-btn" disabled>Iniciar Sesión</button>
            <button class="btn-text" id="go-register">¿No tienes cuenta? Regístrate</button>
            <div class="demo-divider"><span>o</span></div>
            <button class="btn-demo" id="demo-btn">Entrar en modo demostración</button>
            <p class="hint" style="text-align:center">Explora la app con datos de ejemplo, sin necesidad de servidor.</p>
        </div>`;

    $('#demo-btn').addEventListener('click', () => {
        Session.enterDemo();
        navigate('#/dashboard');
    });

    const email = $('#email'), password = $('#password'), btn = $('#login-btn');
    const isEmailValid = () => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value);

    function validate() {
        const emailOk = isEmailValid();
        $('#email-error').classList.toggle('hidden', !email.value || emailOk);
        $('#field-email').classList.toggle('error', email.value && !emailOk);
        btn.disabled = !(emailOk && password.value.length > 0);
    }
    email.addEventListener('input', validate);
    password.addEventListener('input', validate);

    btn.addEventListener('click', async () => {
        btn.disabled = true;
        btn.textContent = 'Ingresando...';
        try {
            const auth = await Api.login(email.value.trim(), password.value);
            Session.save(auth);
            navigate('#/dashboard');
        } catch (e) {
            $('#form-error').textContent = e.message;
            $('#form-error').classList.remove('hidden');
            btn.textContent = 'Iniciar Sesión';
            validate();
        }
    });
    $('#go-register').addEventListener('click', () => navigate('#/register'));
}

function renderRegister() {
    app().innerHTML = `
        <div class="auth-container">
            <h1>Crear Cuenta</h1>
            <p class="auth-subtitle">Se crearán automáticamente tus cuentas en COP y USD</p>
            <div class="field">
                <label>Nombre completo</label>
                <input type="text" id="name" autocomplete="name">
            </div>
            <div class="field" id="field-email">
                <label>Correo electrónico</label>
                <input type="email" id="email" autocomplete="email">
                <span class="support hidden" id="email-error">Formato de correo inválido</span>
            </div>
            <div class="field" id="field-password">
                <label>Contraseña (mínimo 6 caracteres)</label>
                <input type="password" id="password" autocomplete="new-password">
                <span class="support hidden" id="password-error">La contraseña debe tener al menos 6 caracteres</span>
            </div>
            <p class="inline-error hidden" id="form-error"></p>
            <button class="btn-primary" id="register-btn" disabled>Registrarme</button>
            <button class="btn-text" id="go-login">Ya tengo una cuenta</button>
        </div>`;

    const name = $('#name'), email = $('#email'), password = $('#password'), btn = $('#register-btn');
    const isEmailValid = () => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value);
    const isPasswordValid = () => password.value.length >= 6;

    function validate() {
        const emailOk = isEmailValid(), passOk = isPasswordValid();
        $('#email-error').classList.toggle('hidden', !email.value || emailOk);
        $('#field-email').classList.toggle('error', email.value && !emailOk);
        $('#password-error').classList.toggle('hidden', !password.value || passOk);
        $('#field-password').classList.toggle('error', password.value && !passOk);
        btn.disabled = !(name.value.trim() && emailOk && passOk);
    }
    [name, email, password].forEach(el => el.addEventListener('input', validate));

    btn.addEventListener('click', async () => {
        btn.disabled = true;
        btn.textContent = 'Creando cuenta...';
        try {
            const auth = await Api.register(name.value.trim(), email.value.trim(), password.value);
            Session.save(auth);
            navigate('#/dashboard');
        } catch (e) {
            $('#form-error').textContent = e.message;
            $('#form-error').classList.remove('hidden');
            btn.textContent = 'Registrarme';
            validate();
        }
    });
    $('#go-login').addEventListener('click', () => navigate('#/login'));
}

// ---------- Vista: Dashboard ----------
function renderDashboard() {
    app().innerHTML = `
        ${topBar('Monetra', null, `
            <button class="icon-btn" id="swap-btn" title="Cambiar moneda base">${icon('swap')}</button>
            <button class="icon-btn" id="logout-btn" title="Cerrar sesión">${icon('logout')}</button>
        `)}
        <div id="dash-body">
            <div class="content">
                <div class="shimmer" style="height:130px"></div>
                <div class="shimmer" style="height:24px;width:60%"></div>
                <div class="shimmer" style="height:70px"></div>
                <div class="shimmer" style="height:70px"></div>
            </div>
        </div>
        <button class="fab" id="fab-add" title="Añadir Movimiento">${icon('add', 28)}</button>`;

    $('#swap-btn').addEventListener('click', () => {
        State.baseCurrency = State.baseCurrency === 'COP' ? 'USD' : 'COP';
        loadDashboard();
    });
    $('#logout-btn').addEventListener('click', () => { Session.clear(); navigate('#/login'); });
    $('#fab-add').addEventListener('click', () => navigate('#/add'));

    loadDashboard();
}

async function loadDashboard() {
    const body = $('#dash-body');
    try {
        const data = await Api.getDashboard(State.baseCurrency);
        State.dashboard = data;
        Session.saveLastRate(data.usdToCopExchangeRate, data.exchangeRateTimestamp);

        const staleWarning = data.isRateCached ? `
            <div class="offline-badge">${icon('cloudOff', 18)}
                <span>Tasa de cambio desactualizada (último valor: ${formatDate(data.exchangeRateTimestamp)})</span>
            </div>` : '';

        const accounts = data.accounts.map(account => `
            <div class="card account-card">
                <div class="name">${esc(account.name)}</div>
                <div class="balance">${formatMoney(account.currentBalance, account.currencyCode)}</div>
            </div>`).join('');

        const transactions = data.recentTransactions.length === 0
            ? emptyState('No tienes movimientos registrados este mes. Toca el botón + para empezar')
            : data.recentTransactions.map(transactionRow).join('');

        body.innerHTML = `
            <div class="content">
                ${staleWarning}
                <div class="card networth-card">
                    <div class="label">Patrimonio Neto (${data.baseCurrency})</div>
                    <div class="amount">${formatMoney(data.totalNetWorth, data.baseCurrency)}</div>
                    <div class="trm">TRM USD/COP: ${data.usdToCopExchangeRate}</div>
                </div>
                <div class="metric-row">
                    <div class="card metric-card">
                        <div class="label" style="color:var(--income)">${icon('trendingUp', 18)} <span style="color:var(--on-surface)">Ingresos del mes</span></div>
                        <div class="value">${formatMoney(data.monthlyIncome, data.baseCurrency)}</div>
                    </div>
                    <div class="card metric-card">
                        <div class="label" style="color:var(--expense)">${icon('trendingDown', 18)} <span style="color:var(--on-surface)">Egresos del mes</span></div>
                        <div class="value">${formatMoney(data.monthlyExpense, data.baseCurrency)}</div>
                    </div>
                </div>
                <h2 class="section-title">Mis Cuentas</h2>
                <div class="accounts-row">${accounts}</div>
                <div class="chip-row">
                    <button class="chip" id="chip-debts">${icon('creditCard', 18)} Deudas</button>
                    <button class="chip" id="chip-savings">${icon('savings', 18)} Metas</button>
                    <button class="chip" id="chip-subs">${icon('subscriptions', 18)} Suscripciones</button>
                    <button class="chip" id="chip-market">${icon('chart', 18)} Mercado</button>
                </div>
                <h2 class="section-title">Movimientos Recientes</h2>
                ${transactions}
            </div>`;

        $('#chip-debts').addEventListener('click', () => navigate('#/debts'));
        $('#chip-savings').addEventListener('click', () => navigate('#/savings'));
        $('#chip-subs').addEventListener('click', () => navigate('#/subscriptions'));
        $('#chip-market').addEventListener('click', () => navigate('#/market'));
    } catch (e) {
        body.innerHTML = errorBanner(e.message);
        $('#retry-btn').addEventListener('click', loadDashboard);
    }
}

function transactionRow(transaction) {
    const isIncome = transaction.type === 1;
    return `
        <div class="card">
            <div class="tx-row">
                <div class="tx-info">
                    <div class="tx-desc">${esc(transaction.description || transaction.categoryName)}</div>
                    <div class="tx-meta">${esc(transaction.categoryName)} · ${esc(transaction.accountName)} · ${formatDate(transaction.date)}</div>
                </div>
                <div class="tx-amount ${isIncome ? 'income' : 'expense'}">
                    ${isIncome ? '+' : '-'}${formatMoney(transaction.amount, transaction.currencyCode)}
                </div>
            </div>
        </div>`;
}

// ---------- Vista: Añadir Movimiento ----------
function renderAddTransaction() {
    app().innerHTML = `
        ${topBar('Añadir Movimiento', '#/dashboard')}
        <div class="content">
            <div class="segmented">
                <button id="type-income">Ingreso</button>
                <button id="type-expense" class="selected">Egreso</button>
            </div>
            <div class="field" id="field-amount">
                <label>Monto</label>
                <input type="number" id="amount" min="0" step="0.01">
                <span class="support hidden" id="amount-error">El monto debe ser un valor positivo mayor a 0</span>
            </div>
            <div class="field">
                <label>Descripción (ej. "Compra Supermercado")</label>
                <input type="text" id="description" maxlength="255">
            </div>
            <div id="suggestion-slot"></div>
            <div class="field">
                <label>Cuenta origen</label>
                <select id="account-select"><option value="">Cargando cuentas...</option></select>
            </div>
            <p class="inline-error hidden" id="form-error"></p>
            <button class="btn-primary" id="save-btn" disabled>Guardar Movimiento</button>
        </div>`;

    let type = 2;
    let suggestion = null;
    let acceptedCategoryId = null;
    let debounceTimer = null;

    const amount = $('#amount'), description = $('#description'),
          accountSelect = $('#account-select'), saveBtn = $('#save-btn');

    // Cuentas desde el dashboard cacheado o desde la API
    (async () => {
        let accounts = State.dashboard?.accounts;
        if (!accounts) {
            try { accounts = (await Api.getDashboard('COP')).accounts; } catch (_) { accounts = []; }
        }
        accountSelect.innerHTML = accounts.length === 0
            ? '<option value="">No hay cuentas disponibles</option>'
            : accounts.map(a => `<option value="${a.id}">${esc(a.name)} (${a.currencyCode})</option>`).join('');
        validate();
    })();

    function validate() {
        const value = parseFloat(amount.value);
        const amountOk = !isNaN(value) && value > 0;
        $('#amount-error').classList.toggle('hidden', !amount.value || amountOk);
        $('#field-amount').classList.toggle('error', amount.value && !amountOk);
        saveBtn.disabled = !(amountOk && accountSelect.value);
    }
    amount.addEventListener('input', validate);
    accountSelect.addEventListener('change', validate);

    $('#type-income').addEventListener('click', () => {
        type = 1;
        $('#type-income').classList.add('selected');
        $('#type-expense').classList.remove('selected');
    });
    $('#type-expense').addEventListener('click', () => {
        type = 2;
        $('#type-expense').classList.add('selected');
        $('#type-income').classList.remove('selected');
    });

    // Chip de sugerencia IA con debounce de 500ms (igual que la app Android)
    description.addEventListener('input', () => {
        suggestion = null;
        acceptedCategoryId = null;
        $('#suggestion-slot').innerHTML = '';
        clearTimeout(debounceTimer);
        if (description.value.length < 4) return;
        debounceTimer = setTimeout(async () => {
            try {
                suggestion = await Api.suggestCategory(description.value);
                renderSuggestionChip();
            } catch (_) { /* la IA es opcional */ }
        }, 500);
    });

    function renderSuggestionChip() {
        if (!suggestion) return;
        const accepted = acceptedCategoryId !== null;
        $('#suggestion-slot').innerHTML = `
            <div class="chip-row">
                <button class="chip ${accepted ? 'selected' : ''}" id="ai-chip">
                    ${icon(accepted ? 'check' : 'ai', 18)}
                    IA sugiere: ${esc(suggestion.categoryName)} (${Math.round(suggestion.confidence * 100)}%)
                </button>
            </div>`;
        $('#ai-chip').addEventListener('click', () => {
            acceptedCategoryId = suggestion.categoryId;
            renderSuggestionChip();
        });
    }

    saveBtn.addEventListener('click', async () => {
        saveBtn.disabled = true;
        saveBtn.textContent = 'Guardando...';
        try {
            await Api.createTransaction({
                accountId: parseInt(accountSelect.value),
                categoryId: acceptedCategoryId,
                amount: parseFloat(amount.value),
                type,
                date: null,
                description: description.value.trim()
            });
            showSnackbar('Movimiento registrado exitosamente.');
            State.dashboard = null;
            navigate('#/dashboard');
        } catch (e) {
            $('#form-error').textContent = e.message;
            $('#form-error').classList.remove('hidden');
            saveBtn.textContent = 'Guardar Movimiento';
            validate();
        }
    });
}

// ---------- Vista: Deudas ----------
function renderDebts() {
    app().innerHTML = `
        ${topBar('Mis Deudas', '#/dashboard')}
        <div id="list-body"><div class="content"><div class="shimmer" style="height:90px"></div><div class="shimmer" style="height:90px"></div></div></div>
        <button class="fab" id="fab-add" title="Registrar deuda">${icon('add', 28)}</button>`;
    $('#fab-add').addEventListener('click', openCreateDebtDialog);
    loadDebts();
}

async function loadDebts() {
    const body = $('#list-body');
    try {
        await ensureAccounts();
        const debts = await Api.getDebts();
        body.innerHTML = `<div class="content">${
            debts.length === 0
                ? emptyState('No tienes deudas registradas. ¡Excelente salud financiera!')
                : debts.map(debt => {
                    const progress = debt.totalAmount > 0
                        ? Math.min(100, Math.max(0, (1 - debt.remainingAmount / debt.totalAmount) * 100)) : 0;
                    return `
                    <div class="card">
                        <div class="row">
                            <div class="spacer">
                                <div style="font-size:20px;font-weight:600">${esc(debt.creditor)}</div>
                                <div class="muted">Tasa ${debt.interestRate}% ${esc(debt.rateType)} · Vence ${formatDate(debt.dueDate)}</div>
                            </div>
                            <button class="icon-btn" style="color:var(--primary)" data-pay="${debt.id}" data-creditor="${esc(debt.creditor)}" title="Abonar">${icon('payments')}</button>
                            <button class="icon-btn" style="color:var(--expense)" data-del="${debt.id}" title="Eliminar">${icon('del')}</button>
                        </div>
                        <div class="progress-track"><div class="progress-bar" style="width:${progress}%"></div></div>
                        <div style="font-size:16px">Saldo: ${formatMoney(debt.remainingAmount, 'COP')} de ${formatMoney(debt.totalAmount, 'COP')}</div>
                        <div class="muted">Cuota mensual estimada: ${formatMoney(debt.estimatedMonthlyPayment, 'COP')}</div>
                    </div>`;
                }).join('')
        }</div>`;
        bindListActions(body, {
            pay: (id, el) => openPayDebtDialog(id, el.dataset.creditor),
            del: async (id) => {
                try { await Api.deleteDebt(id); showSnackbar('Deuda eliminada.'); loadDebts(); }
                catch (e) { showSnackbar(e.message); }
            }
        });
    } catch (e) {
        body.innerHTML = errorBanner(e.message);
        $('#retry-btn').addEventListener('click', loadDebts);
    }
}

function openCreateDebtDialog() {
    openDialog(`
        <h2>Registrar Deuda</h2>
        <div class="field"><label>Entidad acreedora</label><input type="text" id="d-creditor"></div>
        <div class="field"><label>Capital inicial</label><input type="number" id="d-amount" min="0"></div>
        <div class="field"><label>Tasa de interés (%)</label><input type="number" id="d-rate" min="0" step="0.01"></div>
        <div class="chip-row">
            <button class="chip selected" id="d-ea">EA</button>
            <button class="chip" id="d-em">EM</button>
        </div>
        <div class="field"><label>Fecha de vencimiento</label><input type="date" id="d-due"></div>
        <div class="dialog-actions">
            <button class="btn-text" data-close>Cancelar</button>
            <button class="btn-text" id="d-save" disabled>Guardar</button>
        </div>`,
    (dialog) => {
        let rateType = 'EA';
        const creditor = dialog.querySelector('#d-creditor'), amount = dialog.querySelector('#d-amount'),
              rate = dialog.querySelector('#d-rate'), due = dialog.querySelector('#d-due'),
              save = dialog.querySelector('#d-save');

        dialog.querySelector('#d-ea').addEventListener('click', () => {
            rateType = 'EA';
            dialog.querySelector('#d-ea').classList.add('selected');
            dialog.querySelector('#d-em').classList.remove('selected');
        });
        dialog.querySelector('#d-em').addEventListener('click', () => {
            rateType = 'EM';
            dialog.querySelector('#d-em').classList.add('selected');
            dialog.querySelector('#d-ea').classList.remove('selected');
        });

        function validate() {
            save.disabled = !(creditor.value.trim() && parseFloat(amount.value) > 0 &&
                parseFloat(rate.value) >= 0 && due.value);
        }
        [creditor, amount, rate, due].forEach(el => el.addEventListener('input', validate));

        save.addEventListener('click', async () => {
            save.disabled = true;
            try {
                await Api.createDebt({
                    creditor: creditor.value.trim(),
                    totalAmount: parseFloat(amount.value),
                    interestRate: parseFloat(rate.value),
                    rateType,
                    dueDate: due.value + 'T00:00:00'
                });
                closeDialog();
                showSnackbar('Deuda registrada exitosamente.');
                loadDebts();
            } catch (e) { showSnackbar(e.message); save.disabled = false; }
        });
    });
}

function openPayDebtDialog(debtId, creditor) {
    const accounts = State.dashboard?.accounts || [];
    openDialog(`
        <h2>Abonar a ${esc(creditor)}</h2>
        <div class="field"><label>Monto del abono</label><input type="number" id="p-amount" min="0"></div>
        <div class="field"><label>Cuenta origen</label>
            <select id="p-account">${accounts.map(a => `<option value="${a.id}">${esc(a.name)} (${a.currencyCode})</option>`).join('')}</select>
        </div>
        <div class="dialog-actions">
            <button class="btn-text" data-close>Cancelar</button>
            <button class="btn-text" id="p-save" disabled>Abonar</button>
        </div>`,
    (dialog) => {
        const amount = dialog.querySelector('#p-amount'), account = dialog.querySelector('#p-account'),
              save = dialog.querySelector('#p-save');
        amount.addEventListener('input', () => {
            save.disabled = !(parseFloat(amount.value) > 0 && account.value);
        });
        save.addEventListener('click', async () => {
            save.disabled = true;
            try {
                await Api.recordDebtPayment(debtId, {
                    accountId: parseInt(account.value),
                    amount: parseFloat(amount.value)
                });
                closeDialog();
                showSnackbar('Abono aplicado a la deuda.');
                State.dashboard = null;
                loadDebts();
            } catch (e) { showSnackbar(e.message); save.disabled = false; }
        });
    });
}

// ---------- Vista: Metas de Ahorro ----------
function renderSavings() {
    app().innerHTML = `
        ${topBar('Metas de Ahorro', '#/dashboard')}
        <div id="list-body"><div class="content"><div class="shimmer" style="height:90px"></div><div class="shimmer" style="height:90px"></div></div></div>
        <button class="fab" id="fab-add" title="Crear meta">${icon('add', 28)}</button>`;
    $('#fab-add').addEventListener('click', openCreateGoalDialog);
    loadSavings();
}

async function loadSavings() {
    const body = $('#list-body');
    try {
        await ensureAccounts();
        const goals = await Api.getSavingsGoals();
        body.innerHTML = `<div class="content">${
            goals.length === 0
                ? emptyState('No tienes metas de ahorro activas. Crea una con el botón +')
                : goals.map(goal => `
                    <div class="card">
                        <div class="row">
                            <div class="spacer">
                                <div style="font-size:20px;font-weight:600">${esc(goal.name)}</div>
                                <div class="muted">Fecha límite: ${formatDate(goal.deadline)}</div>
                            </div>
                            <button class="icon-btn" style="color:var(--primary)" data-pay="${goal.id}" data-name="${esc(goal.name)}" title="Depositar">${icon('savings')}</button>
                            <button class="icon-btn" style="color:var(--expense)" data-del="${goal.id}" title="Eliminar">${icon('del')}</button>
                        </div>
                        <div class="progress-track"><div class="progress-bar" style="width:${Math.min(100, goal.progressPercentage)}%"></div></div>
                        <div style="font-size:16px">${formatMoney(goal.currentAmount, 'COP')} de ${formatMoney(goal.targetAmount, 'COP')} (${Math.floor(goal.progressPercentage)}%)</div>
                    </div>`).join('')
        }</div>`;
        bindListActions(body, {
            pay: (id, el) => openDepositDialog(id, el.dataset.name),
            del: async (id) => {
                try { await Api.deleteSavingsGoal(id); showSnackbar('Meta eliminada.'); loadSavings(); }
                catch (e) { showSnackbar(e.message); }
            }
        });
    } catch (e) {
        body.innerHTML = errorBanner(e.message);
        $('#retry-btn').addEventListener('click', loadSavings);
    }
}

function openCreateGoalDialog() {
    openDialog(`
        <h2>Nueva Meta de Ahorro</h2>
        <div class="field"><label>Nombre (ej. Viaje a San Andrés)</label><input type="text" id="g-name"></div>
        <div class="field"><label>Monto objetivo</label><input type="number" id="g-target" min="0"></div>
        <div class="field"><label>Fecha límite</label><input type="date" id="g-deadline"></div>
        <div class="dialog-actions">
            <button class="btn-text" data-close>Cancelar</button>
            <button class="btn-text" id="g-save" disabled>Crear</button>
        </div>`,
    (dialog) => {
        const name = dialog.querySelector('#g-name'), target = dialog.querySelector('#g-target'),
              deadline = dialog.querySelector('#g-deadline'), save = dialog.querySelector('#g-save');
        function validate() {
            save.disabled = !(name.value.trim() && parseFloat(target.value) > 0 && deadline.value);
        }
        [name, target, deadline].forEach(el => el.addEventListener('input', validate));
        save.addEventListener('click', async () => {
            save.disabled = true;
            try {
                await Api.createSavingsGoal({
                    name: name.value.trim(),
                    targetAmount: parseFloat(target.value),
                    deadline: deadline.value + 'T00:00:00'
                });
                closeDialog();
                showSnackbar('Meta creada exitosamente.');
                loadSavings();
            } catch (e) { showSnackbar(e.message); save.disabled = false; }
        });
    });
}

function openDepositDialog(goalId, goalName) {
    const accounts = State.dashboard?.accounts || [];
    openDialog(`
        <h2>Depositar en "${esc(goalName)}"</h2>
        <div class="field"><label>Monto a depositar</label><input type="number" id="dp-amount" min="0"></div>
        <div class="field"><label>Cuenta origen</label>
            <select id="dp-account">${accounts.map(a => `<option value="${a.id}">${esc(a.name)} (${a.currencyCode})</option>`).join('')}</select>
        </div>
        <div class="dialog-actions">
            <button class="btn-text" data-close>Cancelar</button>
            <button class="btn-text" id="dp-save" disabled>Depositar</button>
        </div>`,
    (dialog) => {
        const amount = dialog.querySelector('#dp-amount'), account = dialog.querySelector('#dp-account'),
              save = dialog.querySelector('#dp-save');
        amount.addEventListener('input', () => {
            save.disabled = !(parseFloat(amount.value) > 0 && account.value);
        });
        save.addEventListener('click', async () => {
            save.disabled = true;
            try {
                const goal = await Api.depositToGoal(goalId, {
                    accountId: parseInt(account.value),
                    amount: parseFloat(amount.value)
                });
                closeDialog();
                State.dashboard = null;
                if (goal.isCompleted) {
                    openCelebrationDialog(goal.name);
                } else {
                    showSnackbar('Depósito registrado.');
                }
                loadSavings();
            } catch (e) { showSnackbar(e.message); save.disabled = false; }
        });
    });
}

function openCelebrationDialog(goalName) {
    openDialog(`
        <div style="text-align:center">
            <div style="color:var(--primary);margin-bottom:8px">${icon('star', 48)}</div>
            <h2>¡Meta completada!</h2>
            <p style="margin-bottom:16px">Felicitaciones, alcanzaste tu meta "${esc(goalName)}". Tu disciplina financiera está dando frutos.</p>
            <div class="dialog-actions" style="justify-content:center">
                <button class="btn-text" data-close>Ver Progreso</button>
            </div>
        </div>`, () => {});
}

// ---------- Vista: Suscripciones ----------
function renderSubscriptions() {
    app().innerHTML = `
        ${topBar('Suscripciones', '#/dashboard')}
        <div id="list-body"><div class="content"><div class="shimmer" style="height:90px"></div><div class="shimmer" style="height:90px"></div></div></div>
        <button class="fab" id="fab-add" title="Añadir suscripción">${icon('add', 28)}</button>`;
    $('#fab-add').addEventListener('click', openCreateSubscriptionDialog);
    loadSubscriptions();
}

async function loadSubscriptions() {
    const body = $('#list-body');
    try {
        const subscriptions = await Api.getSubscriptions();
        body.innerHTML = `<div class="content">${
            subscriptions.length === 0
                ? emptyState('No tienes suscripciones registradas. Mapea tus gastos fijos con el botón +')
                : subscriptions.map(sub => {
                    const alert = sub.isActive && sub.daysUntilPayment <= 3 ? `
                        <div class="warning-text" style="margin-top:6px">${icon('bell', 16)}
                            ${sub.daysUntilPayment === 0 ? '¡El cobro es hoy!' : `Cobro en ${sub.daysUntilPayment} día(s)`}
                        </div>` : '';
                    return `
                    <div class="card">
                        <div class="row">
                            <div class="spacer">
                                <div style="font-size:20px;font-weight:600">${esc(sub.name)}</div>
                                <div class="muted">${esc(sub.billingCycle)} · Próximo cobro: ${formatDate(sub.nextPaymentDate)}</div>
                            </div>
                            <label class="switch">
                                <input type="checkbox" data-toggle="${sub.id}" ${sub.isActive ? 'checked' : ''}>
                                <span class="track"><span class="thumb"></span></span>
                            </label>
                            <button class="icon-btn" style="color:var(--expense)" data-del="${sub.id}" title="Eliminar">${icon('del')}</button>
                        </div>
                        <div style="font-size:16px;margin-top:8px">${formatMoney(sub.amount, sub.currencyCode)}</div>
                        ${alert}
                    </div>`;
                }).join('')
        }</div>`;

        body.querySelectorAll('[data-toggle]').forEach(el =>
            el.addEventListener('change', async () => {
                try { await Api.toggleSubscription(parseInt(el.dataset.toggle)); loadSubscriptions(); }
                catch (e) { showSnackbar(e.message); loadSubscriptions(); }
            }));
        bindListActions(body, {
            del: async (id) => {
                try { await Api.deleteSubscription(id); showSnackbar('Suscripción eliminada.'); loadSubscriptions(); }
                catch (e) { showSnackbar(e.message); }
            }
        });
    } catch (e) {
        body.innerHTML = errorBanner(e.message);
        $('#retry-btn').addEventListener('click', loadSubscriptions);
    }
}

function openCreateSubscriptionDialog() {
    openDialog(`
        <h2>Nueva Suscripción</h2>
        <div class="field"><label>Nombre (ej. Netflix)</label><input type="text" id="s-name"></div>
        <div class="field"><label>Monto</label><input type="number" id="s-amount" min="0"></div>
        <div class="chip-row" id="s-currency-row">
            <button class="chip selected" data-currency="COP">COP</button>
            <button class="chip" data-currency="USD">USD</button>
        </div>
        <div class="chip-row" id="s-cycle-row">
            <button class="chip" data-cycle="Weekly">Semanal</button>
            <button class="chip selected" data-cycle="Monthly">Mensual</button>
            <button class="chip" data-cycle="Annual">Anual</button>
        </div>
        <div class="field"><label>Próxima fecha de cobro</label><input type="date" id="s-date"></div>
        <div class="dialog-actions">
            <button class="btn-text" data-close>Cancelar</button>
            <button class="btn-text" id="s-save" disabled>Guardar</button>
        </div>`,
    (dialog) => {
        let currency = 'COP', cycle = 'Monthly';
        const name = dialog.querySelector('#s-name'), amount = dialog.querySelector('#s-amount'),
              date = dialog.querySelector('#s-date'), save = dialog.querySelector('#s-save');

        dialog.querySelectorAll('#s-currency-row .chip').forEach(chip =>
            chip.addEventListener('click', () => {
                currency = chip.dataset.currency;
                dialog.querySelectorAll('#s-currency-row .chip').forEach(c => c.classList.remove('selected'));
                chip.classList.add('selected');
            }));
        dialog.querySelectorAll('#s-cycle-row .chip').forEach(chip =>
            chip.addEventListener('click', () => {
                cycle = chip.dataset.cycle;
                dialog.querySelectorAll('#s-cycle-row .chip').forEach(c => c.classList.remove('selected'));
                chip.classList.add('selected');
            }));

        function validate() {
            save.disabled = !(name.value.trim() && parseFloat(amount.value) > 0 && date.value);
        }
        [name, amount, date].forEach(el => el.addEventListener('input', validate));

        save.addEventListener('click', async () => {
            save.disabled = true;
            try {
                await Api.createSubscription({
                    name: name.value.trim(),
                    amount: parseFloat(amount.value),
                    currencyCode: currency,
                    billingCycle: cycle,
                    nextPaymentDate: date.value + 'T00:00:00'
                });
                closeDialog();
                showSnackbar('Suscripción registrada exitosamente.');
                loadSubscriptions();
            } catch (e) { showSnackbar(e.message); save.disabled = false; }
        });
    });
}

// ---------- Componentes compartidos ----------
function topBar(title, backRoute = null, actions = '') {
    const backBtn = backRoute
        ? `<button class="icon-btn" onclick="location.hash='${backRoute}'" title="Volver">${icon('back')}</button>` : '';
    return `<div class="top-bar">${backBtn}<h1>${esc(title)}</h1>${actions}</div>`;
}

function emptyState(message) {
    return `
        <div class="empty-state">
            <svg viewBox="0 0 24 24">${Icons.receipt}</svg>
            <p>${esc(message)}</p>
        </div>`;
}

function errorBanner(message) {
    return `
        <div class="error-banner">
            <span>${esc(message)}</span>
            <button id="retry-btn">Reintentar</button>
        </div>`;
}

function bindListActions(container, handlers) {
    if (handlers.pay) {
        container.querySelectorAll('[data-pay]').forEach(el =>
            el.addEventListener('click', () => handlers.pay(parseInt(el.dataset.pay), el)));
    }
    if (handlers.del) {
        container.querySelectorAll('[data-del]').forEach(el =>
            el.addEventListener('click', () => handlers.del(parseInt(el.dataset.del))));
    }
}

function openDialog(innerHtml, setup) {
    closeDialog();
    const overlay = document.createElement('div');
    overlay.className = 'dialog-overlay';
    overlay.id = 'dialog';
    overlay.innerHTML = `<div class="dialog-surface">${innerHtml}</div>`;
    document.body.appendChild(overlay);
    overlay.addEventListener('click', (event) => { if (event.target === overlay) closeDialog(); });
    overlay.querySelectorAll('[data-close]').forEach(btn => btn.addEventListener('click', closeDialog));
    setup(overlay);
}

function closeDialog() {
    document.getElementById('dialog')?.remove();
}
