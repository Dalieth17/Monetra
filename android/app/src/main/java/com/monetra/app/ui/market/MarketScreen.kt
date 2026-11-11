package com.monetra.app.ui.market

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.clickable
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.ArrowDropDown
import androidx.compose.material.icons.filled.ArrowDropUp
import androidx.compose.material.icons.filled.Remove
import androidx.compose.material3.Card
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.graphics.nativeCanvas
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.monetra.app.core.CurrencyFormatter
import com.monetra.app.core.DateUtils
import com.monetra.app.data.remote.dto.StockPricePointDto
import com.monetra.app.data.remote.dto.StockQuoteDto
import com.monetra.app.ui.common.EmptyState
import com.monetra.app.ui.common.ErrorBanner
import com.monetra.app.ui.common.OfflineBadge
import com.monetra.app.ui.common.ShimmerBox
import com.monetra.app.ui.theme.ExpenseRed
import com.monetra.app.ui.theme.IncomeGreen
import java.util.Locale
import kotlin.math.roundToInt

// Serie validada contra la superficie de la tarjeta en modo claro y oscuro.
private val ChartSeriesLight = Color(0xFF2A78D6)
private val ChartSeriesDark = Color(0xFF3987E5)
private val ChartDeemphasis = Color(0xFF898781)

@Composable
private fun chartSeriesColor(): Color =
    if (MaterialTheme.colorScheme.surface.luminance() < 0.5f) ChartSeriesDark else ChartSeriesLight

private fun Color.luminance(): Float = 0.2126f * red + 0.7152f * green + 0.0722f * blue

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MarketScreen(
    onNavigateBack: () -> Unit,
    viewModel: MarketViewModel = viewModel(factory = MarketViewModel.Factory)
) {
    val state by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Mercado") },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                }
            )
        }
    ) { padding ->
        Column(modifier = Modifier.padding(padding).fillMaxSize()) {
            state.errorMessage?.let { ErrorBanner(message = it, onRetry = viewModel::load) }

            if (state.isLoading) {
                Column(Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    ShimmerBox(Modifier.fillMaxWidth(), height = 96.dp)
                    repeat(3) { ShimmerBox(Modifier.fillMaxWidth(), height = 64.dp) }
                }
                return@Column
            }

            val rate = state.rate ?: return@Column

            LazyColumn(
                contentPadding = PaddingValues(16.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                if (rate.isStale) {
                    item { OfflineBadge(rate.warningMessage) }
                }
                item {
                    Card(Modifier.fillMaxWidth()) {
                        Column(Modifier.padding(16.dp)) {
                            Text("Dólar estadounidense (USD/COP)", style = MaterialTheme.typography.labelSmall)
                            Spacer(Modifier.height(6.dp))
                            Text(
                                CurrencyFormatter.format(rate.rate, "COP"),
                                style = MaterialTheme.typography.headlineMedium
                            )
                            Text(
                                "Actualizado: ${DateUtils.millisToDisplay(DateUtils.parseIsoToMillis(rate.lastUpdated))}",
                                style = MaterialTheme.typography.labelSmall,
                                color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                            )
                        }
                    }
                }
                item {
                    Text("Acciones", style = MaterialTheme.typography.titleLarge)
                    Text(
                        "Variación frente al cierre anterior. Toca una acción para ver el último mes.",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onBackground.copy(alpha = 0.6f)
                    )
                }
                if (state.stocks.isEmpty()) {
                    item {
                        EmptyState("Las cotizaciones no están disponibles en este momento. Tus cuentas y movimientos siguen funcionando con normalidad.")
                    }
                } else {
                    items(state.stocks, key = { it.symbol }) { quote ->
                        StockCard(
                            quote = quote,
                            expanded = state.expandedSymbol == quote.symbol,
                            onToggle = { viewModel.toggleExpanded(quote.symbol) }
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun StockCard(quote: StockQuoteDto, expanded: Boolean, onToggle: () -> Unit) {
    val direction = quote.change.signum()
    Card(Modifier.fillMaxWidth()) {
        Column {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .clickable(onClick = onToggle)
                    .padding(horizontal = 16.dp, vertical = 14.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                Column(Modifier.weight(1f)) {
                    Text(
                        quote.name,
                        style = MaterialTheme.typography.bodyLarge,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                    Text(
                        quote.symbol,
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                    )
                }
                Sparkline(quote.history, Modifier.width(84.dp).height(32.dp))
                Column(horizontalAlignment = Alignment.End, modifier = Modifier.width(112.dp)) {
                    Text(
                        CurrencyFormatter.format(quote.price, quote.currency),
                        style = MaterialTheme.typography.bodyLarge
                    )
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Icon(
                            imageVector = when {
                                direction > 0 -> Icons.Default.ArrowDropUp
                                direction < 0 -> Icons.Default.ArrowDropDown
                                else -> Icons.Default.Remove
                            },
                            contentDescription = when {
                                direction > 0 -> "Sube"
                                direction < 0 -> "Baja"
                                else -> "Sin cambio"
                            },
                            tint = when {
                                direction > 0 -> IncomeGreen
                                direction < 0 -> ExpenseRed
                                else -> ChartDeemphasis
                            },
                            modifier = Modifier.size(18.dp)
                        )
                        Text(
                            CurrencyFormatter.formatDelta(quote.change, quote.changePercent),
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
                        )
                    }
                }
            }
            if (quote.isStale) {
                OfflineBadge(quote.warningMessage, Modifier.padding(horizontal = 16.dp).padding(bottom = 12.dp))
            }
            if (expanded) {
                StockDetail(quote, Modifier.padding(horizontal = 16.dp).padding(bottom = 14.dp))
            }
        }
    }
}

@Composable
private fun Sparkline(history: List<StockPricePointDto>, modifier: Modifier) {
    if (history.size < 2) return
    val series = chartSeriesColor()
    val surface = MaterialTheme.colorScheme.surface
    val closes = history.map { it.close.toFloat() }
    val low = closes.min()
    val high = closes.max()

    Canvas(modifier) {
        val pad = 6.dp.toPx()
        val span = (high - low).takeIf { it > 0f } ?: 1f
        val stepX = (size.width - 2 * pad) / (closes.size - 1)
        val points = closes.mapIndexed { index, value ->
            Offset(pad + index * stepX, size.height - pad - (value - low) / span * (size.height - 2 * pad))
        }
        val path = Path().apply {
            moveTo(points.first().x, points.first().y)
            points.drop(1).forEach { lineTo(it.x, it.y) }
        }
        drawPath(path, ChartDeemphasis, style = Stroke(2.dp.toPx(), cap = StrokeCap.Round, join = StrokeJoin.Round))
        drawCircle(surface, radius = 6.dp.toPx(), center = points.last())
        drawCircle(series, radius = 4.dp.toPx(), center = points.last())
    }
}

@Composable
private fun StockDetail(quote: StockQuoteDto, modifier: Modifier) {
    val history = quote.history
    if (history.size < 2) {
        Text("Sin histórico disponible para esta acción.", style = MaterialTheme.typography.labelSmall, modifier = modifier)
        return
    }

    var selected by remember(quote.symbol) { mutableIntStateOf(-1) }
    var showTable by remember(quote.symbol) { mutableStateOf(false) }

    Column(modifier) {
        if (selected >= 0) {
            val point = history[selected]
            Text(
                CurrencyFormatter.format(point.close, quote.currency),
                style = MaterialTheme.typography.titleLarge
            )
            Text(
                DateUtils.millisToDisplay(DateUtils.parseIsoToMillis(point.date)),
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
            )
        } else {
            Text("Precio de cierre del último mes", style = MaterialTheme.typography.labelSmall)
            Text(
                "Desliza sobre la gráfica para ver cada sesión",
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
            )
        }
        Spacer(Modifier.height(8.dp))
        PriceChart(
            history = history,
            selectedIndex = selected,
            onSelect = { selected = it },
            modifier = Modifier.fillMaxWidth().height(190.dp)
        )
        Text(
            "Último precio registrado: ${DateUtils.millisToDisplay(DateUtils.parseIsoToMillis(quote.lastUpdated))}",
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
        )
        TextButton(onClick = { showTable = !showTable }, contentPadding = PaddingValues(0.dp)) {
            Text(if (showTable) "Ocultar tabla" else "Ver datos en tabla")
        }
        if (showTable) {
            history.forEach { point ->
                Row(Modifier.fillMaxWidth().padding(vertical = 3.dp)) {
                    Text(
                        DateUtils.millisToDisplay(DateUtils.parseIsoToMillis(point.date)),
                        style = MaterialTheme.typography.labelSmall,
                        modifier = Modifier.weight(1f)
                    )
                    Text(CurrencyFormatter.format(point.close, quote.currency), style = MaterialTheme.typography.labelSmall)
                }
            }
        }
    }
}

@Composable
private fun PriceChart(
    history: List<StockPricePointDto>,
    selectedIndex: Int,
    onSelect: (Int) -> Unit,
    modifier: Modifier
) {
    val series = chartSeriesColor()
    val surface = MaterialTheme.colorScheme.surface
    val muted = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.6f)
    val grid = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.12f)
    val closes = history.map { it.close.toFloat() }
    val scale = remember(closes) { ChartScale.of(closes.min(), closes.max()) }
    val labelSizePx = with(LocalDensity.current) { 10.sp.toPx() }
    val tickFormat = remember(scale.decimals) { "%.${scale.decimals}f" }

    Canvas(
        modifier.pointerInput(history) {
            val left = 46.dp.toPx()
            val right = 14.dp.toPx()
            fun indexAt(x: Float): Int {
                val plotWidth = size.width - left - right
                val fraction = ((x - left) / plotWidth).coerceIn(0f, 1f)
                return (fraction * (history.size - 1)).roundToInt()
            }
            detectDragGestures(onDragStart = { onSelect(indexAt(it.x)) }) { change, _ -> onSelect(indexAt(change.position.x)) }
        }.pointerInput(history) {
            val left = 46.dp.toPx()
            val right = 14.dp.toPx()
            detectTapGestures { offset ->
                val plotWidth = size.width - left - right
                val fraction = ((offset.x - left) / plotWidth).coerceIn(0f, 1f)
                onSelect((fraction * (history.size - 1)).roundToInt())
            }
        }
    ) {
        val left = 46.dp.toPx()
        val right = 14.dp.toPx()
        val top = 12.dp.toPx()
        val bottom = 24.dp.toPx()
        val baseline = size.height - bottom
        val plotWidth = size.width - left - right
        val plotHeight = baseline - top

        val labelPaint = android.graphics.Paint().apply {
            color = muted.toArgb()
            textSize = labelSizePx
            isAntiAlias = true
        }

        scale.ticks.forEach { tick ->
            val y = baseline - scale.fractionOf(tick) * plotHeight
            drawLine(grid, Offset(left, y), Offset(size.width - right, y), strokeWidth = 1f)
            labelPaint.textAlign = android.graphics.Paint.Align.RIGHT
            drawContext.canvas.nativeCanvas.drawText(
                String.format(Locale.US, tickFormat, tick), left - 6.dp.toPx(), y + labelSizePx / 3, labelPaint
            )
        }

        val stepX = plotWidth / (closes.size - 1)
        val points = closes.mapIndexed { index, value ->
            Offset(left + index * stepX, baseline - scale.fractionOf(value) * plotHeight)
        }
        val line = Path().apply {
            moveTo(points.first().x, points.first().y)
            points.drop(1).forEach { lineTo(it.x, it.y) }
        }
        val area = Path().apply {
            addPath(line)
            lineTo(points.last().x, baseline)
            lineTo(points.first().x, baseline)
            close()
        }
        drawPath(area, series.copy(alpha = 0.1f))
        drawPath(line, series, style = Stroke(2.dp.toPx(), cap = StrokeCap.Round, join = StrokeJoin.Round))
        drawCircle(surface, radius = 6.dp.toPx(), center = points.last())
        drawCircle(series, radius = 4.dp.toPx(), center = points.last())

        labelPaint.textAlign = android.graphics.Paint.Align.LEFT
        drawContext.canvas.nativeCanvas.drawText(
            DateUtils.millisToDisplay(DateUtils.parseIsoToMillis(history.first().date)).substringBeforeLast(' '),
            points.first().x, size.height - 6.dp.toPx(), labelPaint
        )
        labelPaint.textAlign = android.graphics.Paint.Align.RIGHT
        drawContext.canvas.nativeCanvas.drawText(
            DateUtils.millisToDisplay(DateUtils.parseIsoToMillis(history.last().date)).substringBeforeLast(' '),
            points.last().x, size.height - 6.dp.toPx(), labelPaint
        )

        if (selectedIndex in points.indices) {
            val point = points[selectedIndex]
            drawLine(ChartDeemphasis, Offset(point.x, top), Offset(point.x, baseline), strokeWidth = 1f)
            drawCircle(surface, radius = 6.dp.toPx(), center = point)
            drawCircle(series, radius = 4.dp.toPx(), center = point)
        }
    }
}
