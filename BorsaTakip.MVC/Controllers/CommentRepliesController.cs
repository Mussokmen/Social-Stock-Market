using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using BorsaTakip.MVC.Models;
using System;

namespace BorsaTakip.MVC.Controllers
{
    public class CommentRepliesController : Controller
    {
        private readonly HttpClient _httpClient;

        public CommentRepliesController()
        {
            _httpClient = new HttpClient();
        }

        // Yorumun yanıtlarını listele (GET)
        [HttpGet]
        public async Task<IActionResult> Index(int commentId)
        {
            var url = $"https://localhost:7095/api/CommentReplies/bycomment/{commentId}";
            var response = await _httpClient.GetAsync(url);

            List<CommentReplyViewModel> replies = new List<CommentReplyViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                replies = JsonSerializer.Deserialize<List<CommentReplyViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            ViewBag.CommentId = commentId;
            return View(replies);
        }

        // Yanıt oluşturma formu (GET)
        [HttpGet]
        public IActionResult Create(int commentId)
        {
            ViewBag.CommentId = commentId;
            return View();
        }

        // Yeni yanıt oluştur (POST)
        [HttpPost]
        public async Task<IActionResult> Create(CommentReplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CommentId = model.CoinCommentId;
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7095/api/CommentReplies", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Yanıt eklenemedi.");
                ViewBag.CommentId = model.CoinCommentId;
                return View(model);
            }

            return RedirectToAction("Index", new { commentId = model.CoinCommentId });
        }

        // Yorum detaylarını getir (GET)
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

        // Yeni yanıt ekle (POST)
        [HttpPost]
        public async Task<IActionResult> CommentDetails(int parentId, string replyComment, string username)
        {
            if (string.IsNullOrWhiteSpace(replyComment) || string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError("", "Yorum ve kullanıcı adı boş olamaz.");
                return RedirectToAction("CommentDetails", new { id = parentId });
            }

            var newReply = new CommentReplyViewModel
            {
                CoinCommentId = parentId,
                Comment = replyComment,
                Username = username,
                CreatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(newReply);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7095/api/CommentReplies", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Yanıt eklenemedi.");
                // İstersen loglama yapabilirsin
            }

            return RedirectToAction("CommentDetails", new { id = parentId });
        }
    }
}
