using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using BorsaTakip.MVC.Models;
using System.Linq;

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

        ViewBag.JWToken = HttpContext.Session.GetString("JWToken");
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

    [HttpPost]
    public async Task<IActionResult> LikeComment(int id)
    {
        var username = User.Identity?.Name;
        var response = await _httpClient.PostAsync($"https://localhost:7095/api/CoinComments/{id}/like?username={username}", null);
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpGet]
    public async Task<IActionResult> FollowingComments()
    {
        var currentUser = User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUser))
            return RedirectToAction("Login", "Account");

        // Takip edilen kullanıcıları çek
        var followingResponse = await _httpClient.GetAsync($"https://localhost:7095/api/UserProfile/following/{currentUser}");
        var followingUsernames = new List<string>();
        if (followingResponse.IsSuccessStatusCode)
        {
            var followingJson = await followingResponse.Content.ReadAsStringAsync();
            var followingObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(followingJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (followingObj != null && (followingObj.ContainsKey("Users") || followingObj.ContainsKey("users")))
            {
                var usersElement = followingObj.ContainsKey("Users") ? followingObj["Users"] : followingObj["users"];
                if (usersElement is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var u in je.EnumerateArray())
                        followingUsernames.Add(u.GetString());
                }
            }
        }
        // Takip edilenlerin yorumlarını çek
        var allComments = new List<CoinCommentViewModel>();
        foreach (var username in followingUsernames)
        {
            var commentsResponse = await _httpClient.GetAsync($"https://localhost:7095/api/CoinComments/usercomments/{username}");
            if (commentsResponse.IsSuccessStatusCode)
            {
                var commentsJson = await commentsResponse.Content.ReadAsStringAsync();
                var comments = System.Text.Json.JsonSerializer.Deserialize<List<CoinCommentViewModel>>(commentsJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (comments != null)
                    allComments.AddRange(comments);
            }
        }
        // Tarihe göre yeni yorumlar en üstte olacak şekilde sırala
        allComments = allComments.OrderByDescending(c => c.CreatedAt).ToList();
        return View("FollowingComments", allComments);
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
