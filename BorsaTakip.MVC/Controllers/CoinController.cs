using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using BorsaTakip.MVC.Models;  // Model namespace'i

namespace BorsaTakip.MVC.Controllers
{
    public class CoinController : Controller
    {
        private readonly HttpClient _httpClient;

        // Sabit popüler coin listesi
        private static readonly List<(string Id, string Name)> PopularCoins = new List<(string, string)>
        {
            ("bitcoin", "Bitcoin"),
            ("ethereum", "Ethereum"),
            ("tether", "Tether"),
            ("binancecoin", "Binance Coin"),
            ("cardano", "Cardano"),
            ("solana", "Solana"),
            ("ripple", "XRP"),
            ("polkadot", "Polkadot"),
            ("dogecoin", "Dogecoin"),
            ("usd-coin", "USD Coin"),
            ("avalanche", "Avalanche"),
            ("terra-luna", "Terra Luna"),
            ("chainlink", "Chainlink"),
            ("litecoin", "Litecoin"),
            ("bitcoin-cash", "Bitcoin Cash"),
            ("algorand", "Algorand"),
            ("stellar", "Stellar"),
            ("vechain", "VeChain"),
            ("internet-computer", "Internet Computer"),
            ("polygon-pos", "Polygon")
        };

        public CoinController()
        {
            _httpClient = new HttpClient();
        }

        // Arama sayfası (boş view döner)
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        // API endpoint: query ile filtrelenmiş coin listesi döner
        [HttpGet]
        public JsonResult SearchCoins(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new List<object>());

            var filtered = PopularCoins
                .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            return Json(filtered);
        }

        // Coin detayları ve 60 günlük fiyat listesini API’den çekip View’a gönderir
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Search");

            var coinDetailsUrl = $"https://api.coingecko.com/api/v3/coins/{id}";
            var priceHistoryUrl = $"https://api.coingecko.com/api/v3/coins/{id}/market_chart?vs_currency=usd&days=60&interval=daily";

            var coinDetailsResponse = await _httpClient.GetAsync(coinDetailsUrl);
            var priceHistoryResponse = await _httpClient.GetAsync(priceHistoryUrl);

            if (!coinDetailsResponse.IsSuccessStatusCode || !priceHistoryResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Veriler alınamadı.";
                return View(null);
            }

            var coinDetailsJson = await coinDetailsResponse.Content.ReadAsStringAsync();
            var priceHistoryJson = await priceHistoryResponse.Content.ReadAsStringAsync();

            var coinDetails = JsonSerializer.Deserialize<JsonElement>(coinDetailsJson);

            using var doc = JsonDocument.Parse(priceHistoryJson);
            var prices = doc.RootElement.GetProperty("prices");

            var priceList = new List<CoinPricePoint>();
            foreach (var item in prices.EnumerateArray())
            {
                var timestamp = item[0].GetDouble();
                var price = item[1].GetDecimal();
                var date = DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp).DateTime;

                priceList.Add(new CoinPricePoint { Date = date, Price = price });
            }

            // En yeni tarih en üstte olacak şekilde sırala
            priceList = priceList.OrderByDescending(p => p.Date).ToList();

            var model = new CoinDetailsViewModel
            {
                CoinDetails = coinDetails,
                PriceHistory = priceList

            };

            return View(model);
        }
    }
}
