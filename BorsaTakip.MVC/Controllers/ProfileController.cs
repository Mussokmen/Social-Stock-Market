using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BorsaTakip.MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BorsaTakip.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7095/"); // API base URL'niz
        }

        [HttpGet]
        public async Task<IActionResult> Index(string username = null)
        {
            var currentUser = User.Identity?.Name;
            var targetUser = string.IsNullOrEmpty(username) ? currentUser : username;

            if (string.IsNullOrEmpty(targetUser))
                return RedirectToAction("Login", "Account");

            // API'den kullanıcının yorumlarını çek
            var commentsResponse = await _httpClient.GetAsync($"api/CoinComments/usercomments/{targetUser}");
            if (!commentsResponse.IsSuccessStatusCode)
                return View("Error");

            var commentsJson = await commentsResponse.Content.ReadAsStringAsync();
            var comments = JsonSerializer.Deserialize<List<CoinCommentViewModel>>(commentsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CoinCommentViewModel>();

            // API'den bio bilgisini çek
            var bioResponse = await _httpClient.GetAsync($"api/UserProfile/bio/{targetUser}");
            string bio = null;
            if (bioResponse.IsSuccessStatusCode)
            {
                var bioJson = await bioResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(bioJson))
                {
                    try { bio = JsonSerializer.Deserialize<string>(bioJson); }
                    catch { bio = bioJson; }
                }
            }

            var model = new UserProfileViewModel
            {
                Username = targetUser,
                Comments = comments,
                Bio = bio
            };

            // Puanı çek
            var pointsResponse = await _httpClient.GetAsync($"api/UserProfile/points/{targetUser}");
            if (pointsResponse.IsSuccessStatusCode)
            {
                var pointsJson = await pointsResponse.Content.ReadAsStringAsync();
                var pointsObj = JsonSerializer.Deserialize<Dictionary<string, int>>(pointsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (pointsObj != null && (pointsObj.ContainsKey("totalPoints") || pointsObj.ContainsKey("TotalPoints")))
                    model.TotalPoints = pointsObj.ContainsKey("totalPoints") ? pointsObj["totalPoints"] : pointsObj["TotalPoints"];
            }

            // Rozetleri çek
            var badgesResponse = await _httpClient.GetAsync($"api/UserProfile/badges/{targetUser}");
            if (badgesResponse.IsSuccessStatusCode)
            {
                var badgesJson = await badgesResponse.Content.ReadAsStringAsync();
                var badges = JsonSerializer.Deserialize<List<BadgeViewModel>>(badgesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                model.Badges = badges ?? new List<BadgeViewModel>();
            }

            // Takipçi ve takip edilen sayısını çek
            var followersResponse = await _httpClient.GetAsync($"api/UserProfile/followers/{targetUser}");
            if (followersResponse.IsSuccessStatusCode)
            {
                var followersJson = await followersResponse.Content.ReadAsStringAsync();
                var followersObj = JsonSerializer.Deserialize<Dictionary<string, object>>(followersJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (followersObj != null && (followersObj.ContainsKey("Count") || followersObj.ContainsKey("count")))
                {
                    var countElement = followersObj.ContainsKey("Count") ? followersObj["Count"] : followersObj["count"];
                    if (countElement is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Number)
                        model.FollowersCount = je.GetInt32();
                    else
                        model.FollowersCount = Convert.ToInt32(countElement);
                }
            }
            var followingResponse = await _httpClient.GetAsync($"api/UserProfile/following/{targetUser}");
            if (followingResponse.IsSuccessStatusCode)
            {
                var followingJson = await followingResponse.Content.ReadAsStringAsync();
                var followingObj = JsonSerializer.Deserialize<Dictionary<string, object>>(followingJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (followingObj != null && (followingObj.ContainsKey("Count") || followingObj.ContainsKey("count")))
                {
                    var countElement = followingObj.ContainsKey("Count") ? followingObj["Count"] : followingObj["count"];
                    if (countElement is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Number)
                        model.FollowingCount = je.GetInt32();
                    else
                        model.FollowingCount = Convert.ToInt32(countElement);
                }
            }
            model.IsOwnProfile = (currentUser == targetUser);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBio(string Bio)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(Bio))
            {
                ModelState.AddModelError("Bio", "Bio boş olamaz.");
                return await Index();
            }

            var payload = new
            {
                Username = username,
                Bio = Bio
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/UserProfile/updatebio", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Bio güncellenirken bir hata oluştu.");
                return await Index();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile photo)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            if (photo == null || photo.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir fotoğraf seçin.");
                return await Index();
            }

            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(photo.OpenReadStream());
            content.Add(streamContent, "photo", photo.FileName);
            content.Add(new StringContent(username), "username");

            var response = await _httpClient.PostAsync("api/UserProfile/uploadphoto", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Fotoğraf yüklenirken hata oluştu.");
                return await Index();
            }

            return RedirectToAction("Index");
        }
    }
}
