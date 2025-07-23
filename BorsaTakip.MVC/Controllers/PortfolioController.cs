using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using BorsaTakip.MVC.Models;
using System.Linq;

namespace BorsaTakip.MVC.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public PortfolioController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new System.Uri("https://localhost:7095/");
            var response = await client.GetAsync($"api/UserCoinHoldings/{username}");
            var holdings = new List<UserCoinHoldingViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                holdings = JsonSerializer.Deserialize<List<UserCoinHoldingViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // Güncel fiyatları çek (CoinGecko)
            decimal? totalCurrentValue = 0;
            if (holdings.Count > 0)
            {
                // Coin adlarını CoinGecko id formatına çevir (küçük harf, boşluk yok)
                var coinIds = holdings.Select(h => h.CoinName.ToLower().Replace(" ", "-")).Distinct().ToList();
                var idsParam = string.Join(",", coinIds);
                var priceUrl = $"https://api.coingecko.com/api/v3/simple/price?ids={idsParam}&vs_currencies=usd";
                var priceResponse = await client.GetAsync(priceUrl);
                if (priceResponse.IsSuccessStatusCode)
                {
                    var priceJson = await priceResponse.Content.ReadAsStringAsync();
                    var priceDoc = JsonDocument.Parse(priceJson);
                    foreach (var holding in holdings)
                    {
                        var id = holding.CoinName.ToLower().Replace(" ", "-");
                        if (priceDoc.RootElement.TryGetProperty(id, out var coinObj))
                        {
                            var price = coinObj.GetProperty("usd").GetDecimal();
                            holding.CurrentPrice = price;
                            holding.CurrentTotalValue = price * holding.Quantity;
                            totalCurrentValue += holding.CurrentTotalValue;
                        }
                    }
                }
            }

            var model = new PortfolioPageViewModel
            {
                Holdings = holdings,
                NewCoin = new UserCoinHoldingViewModel(),
                TotalCurrentValue = totalCurrentValue
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(PortfolioPageViewModel pageModel)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            // Username alanını ModelState'den çıkar
            ModelState.Remove("NewCoin.Username");

            // Portföyü tekrar çek
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new System.Uri("https://localhost:7095/");
            var responseHoldings = await client.GetAsync($"api/UserCoinHoldings/{username}");
            var holdings = new List<UserCoinHoldingViewModel>();
            if (responseHoldings.IsSuccessStatusCode)
            {
                var json = await responseHoldings.Content.ReadAsStringAsync();
                holdings = JsonSerializer.Deserialize<List<UserCoinHoldingViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (!ModelState.IsValid)
            {
                var errorModel = new PortfolioPageViewModel
                {
                    Holdings = holdings,
                    NewCoin = pageModel.NewCoin
                };
                return View("Index", errorModel);
            }

            var model = pageModel.NewCoin ?? new UserCoinHoldingViewModel();
            model.Username = username;
            var jsonModel = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonModel, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/UserCoinHoldings", content);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Coin eklenemedi.");
                var errorModel = new PortfolioPageViewModel
                {
                    Holdings = holdings,
                    NewCoin = pageModel.NewCoin
                };
                return View("Index", errorModel);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new System.Uri("https://localhost:7095/");
            var response = await client.DeleteAsync($"api/UserCoinHoldings/{id}");
            // Başarılıysa veya değilse Index'e dön
            return RedirectToAction("Index");
        }
    }
} 