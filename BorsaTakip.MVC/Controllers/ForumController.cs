using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using BorsaTakip.MVC.Models;

public class ForumController : Controller
{
    private readonly HttpClient _httpClient;

    public ForumController()
    {
        _httpClient = new HttpClient();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var url = "https://localhost:7095/api/CoinComments/all";
        var response = await _httpClient.GetAsync(url);

        List<CoinCommentViewModel> comments = new List<CoinCommentViewModel>();

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            comments = JsonSerializer.Deserialize<List<CoinCommentViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return View(comments);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromForm] CoinCommentViewModel model)
    {
        model.CreatedAt = System.DateTime.UtcNow;
        model.ParentCommentId = null; // Ana yorum

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://localhost:7095/api/CoinComments", content);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Yorum eklenemedi.");
            var comments = await LoadAllComments();
            return View("Index", comments);
        }

        return RedirectToAction("Index");
    }

    // CommentDetails action eklendi
    [HttpGet]
    public async Task<IActionResult> CommentDetails(int id)
    {
        var url = $"https://localhost:7095/api/CoinComments/detail/{id}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var comment = JsonSerializer.Deserialize<CoinCommentViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (comment == null)
            return NotFound();

        return View(comment);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var url = $"https://localhost:7095/api/CoinComments/detail/{id}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var comment = JsonSerializer.Deserialize<CoinCommentViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (comment == null)
            return NotFound();

        return View(comment);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CoinCommentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"https://localhost:7095/api/CoinComments/update/{model.Id}", content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Yorum güncellenemedi.");
            return View(model);
        }

        return RedirectToAction("Index");
    }

    private async Task<List<CoinCommentViewModel>> LoadAllComments()
    {
        var url = "https://localhost:7095/api/CoinComments/all";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return new List<CoinCommentViewModel>();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CoinCommentViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
