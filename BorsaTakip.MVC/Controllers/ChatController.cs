using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YourNamespace.Controllers
{
    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string COHERE_API_KEY = "lqqpfVRYs4nhGNl7tq3dWCkuysrKgWAqnGUEyLgp";

        public ChatController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetAnswer(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return Json(new { success = false, answer = "Lütfen bir soru girin." });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {COHERE_API_KEY}");

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
            {
                return Json(new { success = false, answer = $"API isteği başarısız oldu: {responseContent}" });
            }

            dynamic result = JsonConvert.DeserializeObject(responseContent);
            string answer = result.choices[0].message.content;

            return Json(new { success = true, answer = answer.Trim() });
        }
    }
}
