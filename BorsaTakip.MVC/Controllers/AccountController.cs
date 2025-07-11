using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BorsaTakip.MVC.Models;

namespace BorsaTakip.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new System.Uri("https://localhost:7095/"); // API adresi
        }

        // Register GET
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Register POST
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/register", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Kayıt başarılı, giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Kayıt hatası: {error}");
            return View(model);
        }

        // Login GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponseModel>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Token'ı session'a kaydet
                HttpContext.Session.SetString("JWToken", loginResponse.Token);

                // Kullanıcı adını da session'a kaydet
                HttpContext.Session.SetString("UserName", model.Username);

                // --- Buradan sonrası cookie authentication için ---

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    // Dilersen başka claimler ekleyebilirsin
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    // Dilersen cookie süresini ayarlayabilirsin:
                    // ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // ----------------------------------------

                return RedirectToAction("Index", "Home"); // Başarılı giriş sonrası yönlendirme
            }

            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        // Logout POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("UserName");

            // Cookie oturumunu kapat
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
