// Modo demostración: datos de ejemplo en memoria, sin backend.
// Replica el contrato de la API .NET para que la interfaz funcione completa.

const RATE = 4150.0;

function iso(daysAgo = 0) {
  const d = new Date();
  d.setUTCDate(d.getUTCDate() - daysAgo);
  return d.toISOString().substring(0, 19);
}

const KEYWORDS = {
  Alimentación: ['éxito', 'exito', 'carulla', 'd1', 'ara', 'supermercado', 'restaurante', 'almuerzo', 'mercado', 'rappi', 'café', 'cafe', 'comida'],
  Transporte: ['uber', 'didi', 'gasolina', 'taxi', 'bus', 'transmilenio', 'peaje', 'vuelo', 'avianca'],
  'Servicios Básicos': ['enel', 'epm', 'claro', 'tigo', 'movistar', 'internet', 'agua', 'luz', 'recibo', 'gas'],
  Salud: ['droguería', 'drogueria', 'farmacia', 'médico', 'medico', 'eps', 'hospital'],
  Entretenimiento: ['cine', 'concierto', 'steam', 'bar', 'cerveza', 'teatro'],
  Suscripciones: ['netflix', 'spotify', 'youtube', 'disney', 'apple', 'chatgpt', 'openai'],
  Educación: ['universidad', 'curso', 'udemy', 'platzi', 'libros', 'matricula'],
  Salario: ['salario', 'sueldo', 'nomina', 'nómina', 'quincena', 'prima'],
  Inversiones: ['dividendo', 'acciones', 'cripto', 'binance', 'cdt']
};
const CATEGORIES = {
  Alimentación: { id: 1, icon: 'restaurant' }, Transporte: { id: 2, icon: 'directions_bus' },
  Vivienda: { id: 3, icon: 'home' }, Salud: { id: 4, icon: 'local_hospital' },
  Entretenimiento: { id: 5, icon: 'movie' }, Educación: { id: 6, icon: 'school' },
  'Servicios Básicos': { id: 7, icon: 'bolt' }, Suscripciones: { id: 8, icon: 'subscriptions' },
  Salario: { id: 9, icon: 'payments' }, Inversiones: { id: 10, icon: 'trending_up' },
  Otros: { id: 11, icon: 'more_horiz' }
};

function classify(description) {
  const text = (description || '').toLowerCase();
  for (const [name, keywords] of Object.entries(KEYWORDS)) {
    const hit = keywords.find((kw) => text.includes(kw));
    if (hit) return { categoryId: CATEGORIES[name].id, categoryName: name, confidence: hit.length > 4 ? 0.95 : 0.9, reason: `Patrón "${hit}"` };
  }
  return { categoryId: 11, categoryName: 'Otros', confidence: 0.3, reason: 'Clasificación general por defecto' };
}

function makeStock(symbol, name, start, drift) {
  const history = [];
  let value = start;
  let seed = symbol.charCodeAt(0) * 7 + symbol.length;
  const day = new Date();
  day.setUTCDate(day.getUTCDate() - 32);
  while (history.length < 22) {
    if (day.getUTCDay() !== 0 && day.getUTCDay() !== 6) {
      seed = (seed * 9301 + 49297) % 233280;
      value = +(value * (1 + drift + (seed / 233280 - 0.5) * 0.03)).toFixed(2);
      history.push({ date: day.toISOString().substring(0, 19), close: value });
    }
    day.setUTCDate(day.getUTCDate() + 1);
  }
  const price = history[history.length - 1].close;
  const previousClose = history[history.length - 2].close;
  const change = +(price - previousClose).toFixed(2);
  return {
    symbol, name, price, previousClose, change,
    changePercent: +((change / previousClose) * 100).toFixed(2),
    currency: 'USD', lastUpdated: history[history.length - 1].date, isStale: false, warningMessage: '', history
  };
}

const Store = {
  reset() {
    this.nextId = 100;
    this.accounts = [
      { id: 1, name: 'Efectivo COP', currencyCode: 'COP', currentBalance: 1850000, createdAt: iso(40) },
      { id: 2, name: 'Cuenta Banco USD', currencyCode: 'USD', currentBalance: 420, createdAt: iso(40) }
    ];
    this.transactions = [
      this.tx(1, 'Salario quincena', 2200000, 1, 9, iso(8)),
      this.tx(1, 'Mercado en Éxito', 185000, 2, 1, iso(6)),
      this.tx(1, 'Mensualidad de Netflix', 38900, 2, 8, iso(4)),
      this.tx(2, 'Suscripción ChatGPT', 20, 2, 8, iso(3)),
      this.tx(1, 'Viaje en Uber', 23500, 2, 2, iso(1))
    ];
    this.debts = [
      { id: 1, creditor: 'Bancolombia', totalAmount: 5000000, remainingAmount: 3200000, interestRate: 1.5, rateType: 'EM', dueDate: iso(-540), estimatedMonthlyPayment: 295000, createdAt: iso(60) }
    ];
    this.goals = [
      this.goal(1, 'Viaje a San Andrés', 2000000, 650000, iso(-150)),
      this.goal(2, 'Fondo de emergencia', 5000000, 5000000, iso(-300))
    ];
    this.subscriptions = [
      { id: 1, name: 'Netflix', amount: 38900, currencyCode: 'COP', billingCycle: 'Monthly', nextPaymentDate: iso(-2), isActive: true, daysUntilPayment: 2, createdAt: iso(40) },
      { id: 2, name: 'Spotify', amount: 16900, currencyCode: 'COP', billingCycle: 'Monthly', nextPaymentDate: iso(-15), isActive: true, daysUntilPayment: 15, createdAt: iso(40) }
    ];
    this.stocks = [
      makeStock('AAPL', 'Apple Inc.', 221, 0.002),
      makeStock('MSFT', 'Microsoft Corporation', 430, -0.001),
      makeStock('NVDA', 'NVIDIA Corporation', 178, 0.004),
      makeStock('EC', 'Ecopetrol S.A.', 8.1, 0.0015)
    ];
  },
  tx(accountId, description, amount, type, categoryId, date) {
    const account = this.accounts.find((a) => a.id === accountId);
    const cat = Object.entries(CATEGORIES).find(([, v]) => v.id === categoryId);
    return {
      id: this.nextId++, accountId, accountName: account.name, currencyCode: account.currencyCode,
      categoryId, categoryName: cat ? cat[0] : 'Sin categoría', categoryIcon: cat ? cat[1].icon : 'more_horiz',
      amount, type, date, description, createdAt: date
    };
  },
  goal(id, name, targetAmount, currentAmount, deadline) {
    return {
      id, name, targetAmount, currentAmount,
      progressPercentage: Math.min(100, Math.round((currentAmount / targetAmount) * 10000) / 100),
      deadline, isCompleted: currentAmount >= targetAmount, createdAt: iso(30)
    };
  },
  account(id) {
    const a = this.accounts.find((x) => x.id === id);
    if (!a) throw new Error('La entidad "Cuenta" no fue encontrada.');
    return a;
  }
};

const delay = (data) => new Promise((resolve) => setTimeout(() => resolve(data), 160));

export const DemoApi = {
  enter() {
    Store.reset();
    return { token: 'demo', userId: 1, name: 'Invitado Demo', email: 'demo@monetra.com', role: 'Usuario' };
  },

  getDashboard(baseCurrency = 'COP') {
    const convert = (amount, from) => (from === baseCurrency ? amount : from === 'USD' ? amount * RATE : amount / RATE);
    const totalNetWorth = Store.accounts.reduce((sum, a) => sum + convert(a.currentBalance, a.currencyCode), 0);
    const monthStart = new Date();
    monthStart.setUTCDate(1);
    monthStart.setUTCHours(0, 0, 0, 0);
    let monthlyIncome = 0, monthlyExpense = 0;
    Store.transactions.forEach((t) => {
      if (new Date(t.date) >= monthStart) {
        const v = convert(t.amount, t.currencyCode);
        if (t.type === 1) monthlyIncome += v; else monthlyExpense += v;
      }
    });
    return delay({
      baseCurrency,
      totalNetWorth: Math.round(totalNetWorth * 100) / 100,
      monthlyIncome: Math.round(monthlyIncome * 100) / 100,
      monthlyExpense: Math.round(monthlyExpense * 100) / 100,
      netSavingsThisMonth: Math.round((monthlyIncome - monthlyExpense) * 100) / 100,
      usdToCopExchangeRate: RATE,
      exchangeRateTimestamp: iso(0),
      isRateCached: false,
      accounts: Store.accounts.map((a) => ({ ...a, convertedBalance: Math.round(convert(a.currentBalance, a.currencyCode) * 100) / 100 })),
      recentTransactions: [...Store.transactions].sort((a, b) => b.date.localeCompare(a.date)).slice(0, 10)
    });
  },

  getAccounts: () => delay([...Store.accounts]),
  createAccount(account) {
    const created = { id: Store.nextId++, name: account.name.trim(), currencyCode: account.currencyCode.toUpperCase(), currentBalance: account.initialBalance || 0, createdAt: iso(0) };
    Store.accounts.push(created);
    if (created.currentBalance > 0) Store.transactions.push(Store.tx(created.id, 'Saldo inicial de apertura', created.currentBalance, 1, 9, iso(0)));
    return delay(created);
  },

  getTransactions: () => delay([...Store.transactions].sort((a, b) => b.date.localeCompare(a.date))),
  createTransaction(tx) {
    const account = Store.account(tx.accountId);
    account.currentBalance += tx.type === 1 ? tx.amount : -tx.amount;
    const cat = Object.entries(CATEGORIES).find(([, v]) => v.id === tx.categoryId);
    const created = {
      id: Store.nextId++, accountId: account.id, accountName: account.name, currencyCode: account.currencyCode,
      categoryId: tx.categoryId ?? null, categoryName: cat ? cat[0] : 'Sin categoría', categoryIcon: cat ? cat[1].icon : 'more_horiz',
      amount: tx.amount, type: tx.type, date: tx.date || iso(0), description: tx.description, createdAt: iso(0)
    };
    Store.transactions.push(created);
    return delay(created);
  },
  deleteTransaction(id) { Store.transactions = Store.transactions.filter((t) => t.id !== id); return delay(true); },

  suggestCategory: (description) => delay(classify(description)),
  getUsdCopRate: () => delay({ baseCurrency: 'USD', targetCurrency: 'COP', rate: RATE, lastUpdated: iso(0), isStale: false, warningMessage: '' }),
  getStocks: () => delay(Store.stocks),

  getDebts: () => delay([...Store.debts]),
  createDebt(debt) {
    const created = { id: Store.nextId++, creditor: debt.creditor, totalAmount: debt.totalAmount, remainingAmount: debt.totalAmount, interestRate: debt.interestRate, rateType: debt.rateType, dueDate: debt.dueDate, estimatedMonthlyPayment: Math.round(debt.totalAmount / 12), createdAt: iso(0) };
    Store.debts.push(created);
    return delay(created);
  },
  recordDebtPayment(id, payment) {
    const debt = Store.debts.find((d) => d.id === id);
    const account = Store.account(payment.accountId);
    debt.remainingAmount = Math.max(0, debt.remainingAmount - payment.amount);
    account.currentBalance -= payment.amount;
    Store.transactions.push(Store.tx(account.id, `Abono a deuda: ${debt.creditor}`, payment.amount, 2, 11, iso(0)));
    return delay(debt);
  },
  deleteDebt(id) { Store.debts = Store.debts.filter((d) => d.id !== id); return delay(true); },

  getSavingsGoals: () => delay([...Store.goals]),
  createSavingsGoal(goal) {
    const created = Store.goal(Store.nextId++, goal.name, goal.targetAmount, 0, goal.deadline);
    Store.goals.push(created);
    return delay(created);
  },
  depositToGoal(id, deposit) {
    const goal = Store.goals.find((g) => g.id === id);
    const account = Store.account(deposit.accountId);
    account.currentBalance -= deposit.amount;
    Object.assign(goal, Store.goal(goal.id, goal.name, goal.targetAmount, goal.currentAmount + deposit.amount, goal.deadline));
    Store.transactions.push(Store.tx(account.id, `Depósito a meta: ${goal.name}`, deposit.amount, 2, 10, iso(0)));
    return delay(goal);
  },
  deleteSavingsGoal(id) { Store.goals = Store.goals.filter((g) => g.id !== id); return delay(true); },

  getSubscriptions: () => delay([...Store.subscriptions]),
  createSubscription(sub) {
    const days = Math.max(0, Math.ceil((new Date(sub.nextPaymentDate) - new Date()) / 86400000));
    const created = { id: Store.nextId++, name: sub.name, amount: sub.amount, currencyCode: sub.currencyCode, billingCycle: sub.billingCycle, nextPaymentDate: sub.nextPaymentDate, isActive: true, daysUntilPayment: days, createdAt: iso(0) };
    Store.subscriptions.push(created);
    return delay(created);
  },
  toggleSubscription(id) {
    const sub = Store.subscriptions.find((s) => s.id === id);
    sub.isActive = !sub.isActive;
    return delay(sub);
  },
  deleteSubscription(id) { Store.subscriptions = Store.subscriptions.filter((s) => s.id !== id); return delay(true); }
};

// Datos listos al cargar, para que un refresco en modo demo no quede sin estado.
Store.reset();
