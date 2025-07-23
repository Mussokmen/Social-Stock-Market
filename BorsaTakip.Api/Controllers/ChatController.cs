using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BorsaTakip.Api.Data;
using BorsaTakip.Api.Models;
using System;
using System.Linq;

namespace BorsaTakip.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BorsaTakipDbContext _dbContext;
        private const string OPENAI_API_KEY = "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";
        private const string OPENROUTER_API_KEY = "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";

        public ChatController(IHttpClientFactory httpClientFactory, BorsaTakipDbContext dbContext)
        {
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QuestionRequest request)
        {
            var question = request?.Question?.Trim();

            if (string.IsNullOrWhiteSpace(question))
                return BadRequest(new { success = false, answer = "Lütfen boş olmayan bir soru girin." });

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {OPENAI_API_KEY}");

            var requestBody = new
            {
                model = "command-xlarge-nightly",
                messages = new[]
                {
                    new { role = "user", content = question }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.cohere.ai/chat", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new { success = false, answer = $"API isteği başarısız oldu: {responseContent}" });

            dynamic result = JsonConvert.DeserializeObject(responseContent);
            string answer = result.choices[0].message.content;

            // Veritabanına kaydet
            var chatHistory = new ChatHistory
            {
                Question = question,
                Answer = answer,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ChatHistories.Add(chatHistory);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, answer = answer.Trim() });
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return BadRequest(new { success = false, answer = "Lütfen bir soru girin." });

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {OPENROUTER_API_KEY}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "https://borsatakip.com");
            client.DefaultRequestHeaders.Add("X-Title", "BorsaTakipChat");

            var messages = new[]
            {
                new { role = "system", content = "Her soruya kesin, net ve tahmin içeren cevaplar ver. Asla 'bilmiyorum' veya 'tavsiye veremem' deme. Cevapların Türkçe ve kısa olsun. Borsa ve kripto para sorularında net tahmin yap." },
                new { role = "user", content = request.Question }
            };

            var requestBody = new
            {
                model = "openai/gpt-3.5-turbo",
                messages = messages,
                max_tokens = 200,
                temperature = 0.7
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new { success = false, answer = $"API hatası: {responseContent}" });

            dynamic result = JsonConvert.DeserializeObject(responseContent);
            string answer = result.choices[0].message.content;

            return Ok(new { success = true, answer = answer.Trim() });
        }

        [HttpGet("history")]
        public IActionResult GetHistory([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Kullanıcı adı gerekli.");
            var history = _dbContext.ChatHistories
                .Where(x => x.Question != null && x.Answer != null && x.Question != "" && x.Answer != "" && x.Question.StartsWith(username + ":"))
                .OrderBy(x => x.CreatedAt)
                .Select(x => new {
                    Question = x.Question.Substring(username.Length + 1),
                    x.Answer,
                    x.CreatedAt
                })
                .ToList();
            return Ok(history);
        }
    }

    public class QuestionRequest
    {
        public string Question { get; set; }
    }

    public class ChatRequest
    {
        public string Question { get; set; }
    }
}
