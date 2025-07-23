using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorsaTakip.Api.Data; // <-- Bunu ekleyin!
using System.Net.Http;
using System.Text.Json;

namespace BorsaTakip.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoinController : ControllerBase
    {
        private readonly BorsaTakipDbContext _context;
        public CoinController(BorsaTakipDbContext context)
        {
            _context = context;
        }

        // GET: api/coin/search?query=b
        [HttpGet("search")]
        public async Task<IActionResult> SearchCoins([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
                return Ok(new List<string>());
            var coins = await _context.UserCoinHoldings
                .Where(c => c.CoinName.ToLower().Contains(query.ToLower()))
                .Select(c => c.CoinName)
                .Distinct()
                .ToListAsync();

            // CoinGecko'dan da arama yap
            var client = new HttpClient();
            var geckoResp = await client.GetAsync($"https://api.coingecko.com/api/v3/coins/list?include_platform=false");
            if (geckoResp.IsSuccessStatusCode)
            {
                var geckoJson = await geckoResp.Content.ReadAsStringAsync();
                var geckoList = JsonSerializer.Deserialize<List<CoinGeckoCoin>>(geckoJson);
                var geckoMatches = geckoList
                    .Where(c => c.name != null && c.name.ToLower().Contains(query.ToLower()))
                    .Select(c => c.name)
                    .Except(coins)
                    .Take(10 - coins.Count)
                    .ToList();
                coins.AddRange(geckoMatches);
            }
            coins = coins.Distinct().Take(10).ToList();
            return Ok(coins);
        }

        public class CoinGeckoCoin
        {
            public string id { get; set; }
            public string symbol { get; set; }
            public string name { get; set; }
        }
    }
} 