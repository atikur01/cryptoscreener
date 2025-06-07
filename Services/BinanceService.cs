using CryptoScreener.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace CryptoScreener.Services
{
    public class BinanceService
    {
        private readonly HttpClient _httpClient;
        private readonly ConcurrentBag<PairData> _cachedData = new();
        private const string API_BASE = "https://api.binance.us/api/v3/";

        public BinanceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IEnumerable<PairData> GetCachedData() => _cachedData.ToList();

        public async Task UpdateDataAsync()
        {
            var newList = new List<PairData>();
            var pairs = await GetUsdtPairs();

            foreach (var pair in pairs)
            {
                await Task.Delay(100);

                try
                {
                    var closes = await GetCloses(pair);
                    if (closes.Count < 14) continue;

                    var rsi = CalculateRsi(closes);
                    var latestRsi = rsi.Last();
                    var rsiStatus = GetRsiStatus(latestRsi);
                    var price = closes.Last();
                    var change = await GetPriceChange(pair);

                    newList.Add(new PairData
                    {
                        Symbol = pair,
                        Price = price,
                        PriceChange = change,
                        RSI = latestRsi,
                        RSIStatus = rsiStatus
                    });
                }
                catch { continue; }
            }

            _cachedData.Clear();
            foreach (var item in newList)
                _cachedData.Add(item);
        }

        private async Task<List<decimal>> GetCloses(string pair)
        {
            var url = $"{API_BASE}klines?symbol={pair}&interval=1d&limit=500";
            var res = await _httpClient.GetStringAsync(url);
            var j = JArray.Parse(res);
            return j.Select(c => decimal.Parse(c[4].ToString())).ToList();
        }

        private async Task<List<string>> GetUsdtPairs()
        {
            var res = await _httpClient.GetStringAsync($"{API_BASE}exchangeInfo");
            var symbols = JObject.Parse(res)["symbols"];

            return symbols
                .Where(s => s["quoteAsset"]?.ToString() == "USDT" && s["status"]?.ToString() == "TRADING")
                .Select(s => s["symbol"]!.ToString())
                .ToList();
        }

        private async Task<decimal> GetPriceChange(string pair)
        {
            var res = await _httpClient.GetStringAsync($"{API_BASE}ticker/24hr?symbol={pair}");
            return decimal.Parse(JObject.Parse(res)["priceChangePercent"]!.ToString());
        }

        private List<decimal> CalculateRsi(List<decimal> closes)
        {
            var period = 14;
            var rsis = new List<decimal>();

            for (int i = period; i < closes.Count; i++)
            {
                var gains = 0.0m;
                var losses = 0.0m;
                for (int j = i - period + 1; j <= i; j++)
                {
                    var change = closes[j] - closes[j - 1];
                    if (change > 0) gains += change;
                    else losses -= change;
                }
                if (losses == 0) rsis.Add(100);
                else
                {
                    var rs = gains / losses;
                    rsis.Add(100 - (100 / (1 + rs)));
                }
            }

            return rsis;
        }

        private string GetRsiStatus(decimal rsi)
        {
            return rsi > 70 ? "Overbought" : rsi < 30 ? "Oversold" : "";
        }
    }
}
