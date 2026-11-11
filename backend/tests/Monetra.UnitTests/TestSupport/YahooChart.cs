using System.Globalization;

namespace Monetra.UnitTests.TestSupport;

/// <summary>
/// Respuestas con la forma de https://query1.finance.yahoo.com/v8/finance/chart/{símbolo}?range=1mo&amp;interval=1d.
/// Cinco sesiones (2 al 6 de noviembre de 2026); la del día 4 viene sin cierre, como ocurre en datos reales.
/// </summary>
internal static class YahooChart
{
    public const string ErrorPayload = """
        {"chart":{"result":null,"error":{"code":"Not Found","description":"No data found, symbol may be delisted"}}}
        """;

    public static string Payload(string symbol = "AAPL", decimal price = 226.96m, bool withNames = true)
    {
        var names = withNames ? """
            "longName": "Apple Inc.",
                    "shortName": "Apple Inc.",
            """ : string.Empty;

        return $$"""
            {
              "chart": {
                "result": [
                  {
                    "meta": {
                      "currency": "USD",
                      "symbol": "{{symbol}}",
                      "exchangeName": "NMS",
                      "fullExchangeName": "NasdaqGS",
                      "instrumentType": "EQUITY",
                      "firstTradeDate": 345479400,
                      "regularMarketTime": 1793998801,
                      "hasPrePostMarketData": true,
                      "gmtoffset": -18000,
                      "timezone": "EST",
                      "exchangeTimezoneName": "America/New_York",
                      "regularMarketPrice": {{price.ToString(CultureInfo.InvariantCulture)}},
                      "fiftyTwoWeekHigh": 237.49,
                      "fiftyTwoWeekLow": 164.08,
                      "regularMarketDayHigh": 228.66,
                      "regularMarketDayLow": 226.41,
                      "regularMarketVolume": 38328824,
                      {{names}}
                      "chartPreviousClose": 219.86,
                      "priceHint": 2,
                      "currentTradingPeriod": {
                        "pre": { "timezone": "EST", "start": 1793955600, "end": 1793975400, "gmtoffset": -18000 },
                        "regular": { "timezone": "EST", "start": 1793975400, "end": 1793998800, "gmtoffset": -18000 },
                        "post": { "timezone": "EST", "start": 1793998800, "end": 1794013200, "gmtoffset": -18000 }
                      },
                      "dataGranularity": "1d",
                      "range": "1mo",
                      "validRanges": ["1d", "5d", "1mo", "3mo", "6mo", "1y", "2y", "5y", "10y", "ytd", "max"]
                    },
                    "timestamp": [1793629800, 1793716200, 1793802600, 1793889000, 1793975400],
                    "indicators": {
                      "quote": [
                        {
                          "open": [220.1, 221.5, null, 224.0, 225.9],
                          "low": [219.8, 221.0, null, 223.6, 226.41],
                          "volume": [41000000, 39500000, null, 36200000, 38328824],
                          "close": [221.25, 223.4500122070312, null, 225.6699981689453, 226.9600067138672],
                          "high": [222.0, 224.1, null, 226.2, 228.66]
                        }
                      ],
                      "adjclose": [
                        { "adjclose": [221.25, 223.4500122070312, null, 225.6699981689453, 226.9600067138672] }
                      ]
                    }
                  }
                ],
                "error": null
              }
            }
            """;
    }
}
