/* Monetra Web - pantalla Mercado: tasa USD/COP y acciones seleccionadas */

Object.assign(Icons, {
    chart: '<path d="M3.5 18.49l6-6.01 4 4L22 6.92l-1.41-1.41-7.09 7.97-4-4L2 16.99z"/>',
    up: '<path d="M7 14l5-5 5 5z"/>',
    down: '<path d="M7 10l5 5 5-5z"/>',
    flat: '<path d="M19 13H5v-2h14v2z"/>'
});

routes['#/market'] = renderMarket;

const CHART = { width: 360, height: 190, left: 46, right: 14, top: 12, bottom: 24 };

function renderMarket() {
    app().innerHTML = `
        ${topBar('Mercado', '#/dashboard')}
        <div id="list-body"><div class="content">
            <div class="shimmer" style="height:96px"></div>
            <div class="shimmer" style="height:64px"></div>
            <div class="shimmer" style="height:64px"></div>
            <div class="shimmer" style="height:64px"></div>
        </div></div>`;
    loadMarket();
}

async function loadMarket() {
    const body = $('#list-body');
    try {
        const [rate, stocks] = await Promise.all([Api.getUsdCopRate(), Api.getStocks()]);

        const staleRate = rate.isStale
            ? `<div class="offline-badge">${icon('cloudOff', 18)}<span>${esc(rate.warningMessage)}</span></div>` : '';

        const stockList = stocks.length === 0
            ? emptyState('Las cotizaciones no están disponibles en este momento. Tus cuentas y movimientos siguen funcionando con normalidad.')
            : stocks.map(stockCard).join('');

        body.innerHTML = `
            <div class="content">
                ${staleRate}
                <div class="card rate-tile">
                    <div class="label">Dólar estadounidense (USD/COP)</div>
                    <div class="value">${formatMoney(rate.rate, 'COP')}</div>
                    <div class="muted">Actualizado: ${formatDate(rate.lastUpdated)}</div>
                </div>
                <h2 class="section-title">Acciones</h2>
                <p class="hint" style="margin:-4px 0 12px">Variación frente al cierre anterior. Toca una acción para ver el último mes.</p>
                ${stockList}
            </div>`;

        body.querySelectorAll('.stock-head').forEach(head => {
            head.addEventListener('click', () => {
                const quote = stocks.find(q => q.symbol === head.dataset.symbol);
                toggleStockDetail(head, quote);
            });
        });
    } catch (e) {
        body.innerHTML = errorBanner(e.message);
        $('#retry-btn').addEventListener('click', loadMarket);
    }
}

// ---------- Ficha de acción: valor + variación + minigráfica ----------
function stockCard(quote) {
    const direction = quote.change > 0 ? 'up' : quote.change < 0 ? 'down' : 'flat';
    const staleNote = quote.isStale
        ? `<div class="offline-badge" style="margin:0 16px 12px">${icon('cloudOff', 18)}<span>${esc(quote.warningMessage)}</span></div>` : '';

    return `
        <div class="card stock-card">
            <button class="stock-head" data-symbol="${esc(quote.symbol)}" aria-expanded="false">
                <div class="stock-id">
                    <div class="stock-name">${esc(quote.name)}</div>
                    <div class="muted">${esc(quote.symbol)}</div>
                </div>
                <div class="stock-spark">${sparkline(quote.history)}</div>
                <div class="stock-figures">
                    <div class="stock-price">${formatMoney(quote.price, quote.currency)}</div>
                    <div class="stock-delta delta-${direction}">${icon(direction, 18)}${formatDelta(quote)}</div>
                </div>
            </button>
            ${staleNote}
            <div class="stock-detail hidden"></div>
        </div>`;
}

function formatDelta(quote) {
    const sign = quote.change > 0 ? '+' : quote.change < 0 ? '−' : '';
    return `${sign}${Math.abs(quote.change).toFixed(2)} (${sign}${Math.abs(quote.changePercent).toFixed(2)}%)`;
}

function scalePoints(values, width, height, padX, padTop, padBottom, low, high) {
    const span = (high - low) || 1;
    const stepX = values.length > 1 ? (width - 2 * padX) / (values.length - 1) : 0;
    return values.map((value, index) => ({
        x: padX + index * stepX,
        y: height - padBottom - ((value - low) / span) * (height - padTop - padBottom)
    }));
}

function sparkline(history) {
    if (history.length < 2) return '';
    const closes = history.map(point => point.close);
    const points = scalePoints(closes, 84, 32, 6, 6, 6, Math.min(...closes), Math.max(...closes));
    const last = points[points.length - 1];
    return `
        <svg viewBox="0 0 84 32" aria-hidden="true">
            <polyline points="${points.map(p => `${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(' ')}"
                fill="none" stroke="var(--chart-deemphasis)" stroke-width="2" stroke-linejoin="round" stroke-linecap="round"/>
            <circle cx="${last.x.toFixed(1)}" cy="${last.y.toFixed(1)}" r="4"
                fill="var(--chart-series)" stroke="var(--surface)" stroke-width="2"/>
        </svg>`;
}

// ---------- Detalle: gráfica de línea del último mes ----------
function toggleStockDetail(head, quote) {
    const detail = head.parentElement.querySelector('.stock-detail');
    const willOpen = detail.classList.contains('hidden');
    head.setAttribute('aria-expanded', String(willOpen));
    detail.classList.toggle('hidden', !willOpen);
    if (willOpen && !detail.dataset.rendered) {
        renderStockDetail(detail, quote);
        detail.dataset.rendered = 'true';
    }
}

function niceTicks(min, max) {
    if (min === max) { min -= 1; max += 1; }
    const rough = (max - min) / 3;
    const magnitude = Math.pow(10, Math.floor(Math.log10(rough)));
    const step = [1, 2, 2.5, 5, 10].map(m => m * magnitude).find(s => s >= rough);
    const low = Math.floor(min / step) * step;
    const high = Math.ceil(max / step) * step;
    const ticks = [];
    for (let value = low; value <= high + step / 2; value += step) ticks.push(value);
    const decimals = step >= 1 ? (Number.isInteger(step) ? 0 : 1) : 2;
    return { low, high, ticks, decimals };
}

function shortDate(isoString) {
    return new Date(isoString.substring(0, 19)).toLocaleDateString('es-CO', { day: 'numeric', month: 'short' });
}

function renderStockDetail(container, quote) {
    const history = quote.history;
    if (history.length < 2) {
        container.innerHTML = '<p class="muted">Sin histórico disponible para esta acción.</p>';
        return;
    }

    const { width, height, left, right, top, bottom } = CHART;
    const closes = history.map(point => point.close);
    const scale = niceTicks(Math.min(...closes), Math.max(...closes));
    const baseline = height - bottom;
    const plotWidth = width - left - right;
    const points = scalePoints(closes, plotWidth, height, 0, top, bottom, scale.low, scale.high)
        .map(p => ({ x: p.x + left, y: p.y }));
    const path = points.map(p => `${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(' ');
    const last = points[points.length - 1];
    const tickFormat = new Intl.NumberFormat('en-US', {
        minimumFractionDigits: scale.decimals, maximumFractionDigits: scale.decimals
    });

    const grid = scale.ticks.map(tick => {
        const y = baseline - ((tick - scale.low) / (scale.high - scale.low)) * (baseline - top);
        return `
            <line x1="${left}" x2="${width - right}" y1="${y.toFixed(1)}" y2="${y.toFixed(1)}" stroke="var(--chart-grid)" stroke-width="1"/>
            <text x="${left - 6}" y="${(y + 3.5).toFixed(1)}" text-anchor="end">${tickFormat.format(tick)}</text>`;
    }).join('');

    const rows = history.map(point => `
        <tr><td>${formatDate(point.date)}</td><td>${formatMoney(point.close, quote.currency)}</td></tr>`).join('');

    container.innerHTML = `
        <div class="chart-wrap" tabindex="0" role="group"
             aria-label="Precio de cierre del último mes. Usa las flechas izquierda y derecha para recorrer las sesiones.">
            <svg viewBox="0 0 ${width} ${height}" aria-hidden="true">
                ${grid}
                <polygon points="${points[0].x.toFixed(1)},${baseline} ${path} ${last.x.toFixed(1)},${baseline}"
                    fill="var(--chart-series)" fill-opacity="0.1"/>
                <polyline points="${path}" fill="none" stroke="var(--chart-series)" stroke-width="2"
                    stroke-linejoin="round" stroke-linecap="round"/>
                <circle cx="${last.x.toFixed(1)}" cy="${last.y.toFixed(1)}" r="4"
                    fill="var(--chart-series)" stroke="var(--surface)" stroke-width="2"/>
                <text x="${points[0].x}" y="${height - 6}" text-anchor="start">${shortDate(history[0].date)}</text>
                <text x="${last.x}" y="${height - 6}" text-anchor="end">${shortDate(history[history.length - 1].date)}</text>
                <g class="crosshair" visibility="hidden">
                    <line y1="${top}" y2="${baseline}" stroke="var(--chart-deemphasis)" stroke-width="1"/>
                    <circle r="4" fill="var(--chart-series)" stroke="var(--surface)" stroke-width="2"/>
                </g>
            </svg>
            <div class="chart-tooltip hidden" role="status" aria-live="polite"></div>
        </div>
        <div class="muted">Último precio registrado: ${formatDate(quote.lastUpdated)}</div>
        <details class="chart-table">
            <summary>Ver datos en tabla</summary>
            <table>
                <thead><tr><th>Fecha</th><th>Cierre</th></tr></thead>
                <tbody>${rows}</tbody>
            </table>
        </details>`;

    bindChartHover(container.querySelector('.chart-wrap'), points, history, quote.currency);
}

/** Cursor vertical que salta a la sesión más cercana; mismo detalle con el teclado. */
function bindChartHover(wrap, points, history, currency) {
    const svg = wrap.querySelector('svg');
    const crosshair = wrap.querySelector('.crosshair');
    const tooltip = wrap.querySelector('.chart-tooltip');
    let active = -1;

    function show(index) {
        active = Math.max(0, Math.min(points.length - 1, index));
        const point = points[active];
        crosshair.setAttribute('visibility', 'visible');
        crosshair.querySelector('line').setAttribute('x1', point.x);
        crosshair.querySelector('line').setAttribute('x2', point.x);
        crosshair.querySelector('circle').setAttribute('cx', point.x);
        crosshair.querySelector('circle').setAttribute('cy', point.y);

        const value = document.createElement('strong');
        value.textContent = formatMoney(history[active].close, currency);
        const date = document.createElement('span');
        date.textContent = formatDate(history[active].date);
        tooltip.replaceChildren(value, date);

        const xPercent = Math.max(18, Math.min(82, (point.x / CHART.width) * 100));
        const below = point.y < CHART.height * 0.4;
        tooltip.style.left = `${xPercent}%`;
        tooltip.style.top = `${(point.y / CHART.height) * 100}%`;
        tooltip.style.transform = `translate(-50%, ${below ? '14px' : 'calc(-100% - 14px)'})`;
        tooltip.classList.remove('hidden');
    }

    function hide() {
        active = -1;
        crosshair.setAttribute('visibility', 'hidden');
        tooltip.classList.add('hidden');
    }

    function nearestIndex(clientX) {
        const box = svg.getBoundingClientRect();
        const x = ((clientX - box.left) / box.width) * CHART.width;
        const step = (points[points.length - 1].x - points[0].x) / (points.length - 1);
        return Math.round((x - points[0].x) / step);
    }

    wrap.addEventListener('pointermove', event => show(nearestIndex(event.clientX)));
    wrap.addEventListener('pointerdown', event => show(nearestIndex(event.clientX)));
    wrap.addEventListener('pointerleave', () => { if (document.activeElement !== wrap) hide(); });
    wrap.addEventListener('focus', () => { if (active < 0) show(points.length - 1); });
    wrap.addEventListener('blur', hide);
    wrap.addEventListener('keydown', event => {
        const moves = { ArrowLeft: active - 1, ArrowRight: active + 1, Home: 0, End: points.length - 1 };
        if (event.key in moves) {
            event.preventDefault();
            show(moves[event.key]);
        }
    });
}
