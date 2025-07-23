using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BorsaTakip.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BorsaTakip.MVC.Controllers
{
    public class BadgesController : Controller
    {
        private readonly HttpClient _httpClient;
        public BadgesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7095/");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            // Puanları çek
            var pointsResponse = await _httpClient.GetAsync($"api/UserProfile/points/{username}");
            var points = new BadgePointsViewModel();
            if (pointsResponse.IsSuccessStatusCode)
            {
                var pointsJson = await pointsResponse.Content.ReadAsStringAsync();
                points = JsonSerializer.Deserialize<BadgePointsViewModel>(pointsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new BadgePointsViewModel();
            }

            // Yorumlar
            var commentsResponse = await _httpClient.GetAsync($"api/CoinComments/usercomments/{username}");
            var comments = new List<CoinCommentViewModel>();
            if (commentsResponse.IsSuccessStatusCode)
            {
                var commentsJson = await commentsResponse.Content.ReadAsStringAsync();
                comments = JsonSerializer.Deserialize<List<CoinCommentViewModel>>(commentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CoinCommentViewModel>();
            }
            // Yanıtlar
            var repliesResponse = await _httpClient.GetAsync($"api/CommentReplies/byuser/{username}");
            var replies = new List<CommentReplyViewModel>();
            if (repliesResponse.IsSuccessStatusCode)
            {
                var repliesJson = await repliesResponse.Content.ReadAsStringAsync();
                replies = JsonSerializer.Deserialize<List<CommentReplyViewModel>>(repliesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CommentReplyViewModel>();
            }
            // Rozetler
            var badgesResponse = await _httpClient.GetAsync($"api/UserProfile/badges/{username}");
            var earnedBadges = new List<BadgeViewModel>();
            if (badgesResponse.IsSuccessStatusCode)
            {
                var badgesJson = await badgesResponse.Content.ReadAsStringAsync();
                earnedBadges = JsonSerializer.Deserialize<List<BadgeViewModel>>(badgesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<BadgeViewModel>();
            }

            // Rozet ilerlemeleri
            var progress = new List<BadgeProgressViewModel>();
            // 1. Popüler Yorumcu
            int populerYorum = comments.Any() ? comments.Max(c => c.LikeCount) : 0;
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "PopulerYorumcu",
                BadgeName = "Popüler Yorumcu",
                Description = "10+ beğeni alan yorum yaz.",
                CurrentValue = populerYorum,
                TargetValue = 10,
                Earned = earnedBadges.Any(b => b.BadgeType == "PopulerYorumcu")
            });
            // 2. Sürekli Yorumcu
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "SurekliYorumcu",
                BadgeName = "Sürekli Yorumcu",
                Description = "Toplam 20 yorum yaz.",
                CurrentValue = comments.Count,
                TargetValue = 20,
                Earned = earnedBadges.Any(b => b.BadgeType == "SurekliYorumcu")
            });
            // 3. Cevap Ustası
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "CevapUstasi",
                BadgeName = "Cevap Ustası",
                Description = "10+ yanıt yaz.",
                CurrentValue = replies.Count,
                TargetValue = 10,
                Earned = earnedBadges.Any(b => b.BadgeType == "CevapUstasi")
            });
            // 4. Beğeni Avcısı
            int toplamBegeni = comments.Sum(c => c.LikeCount);
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "BegeniAvcisi",
                BadgeName = "Beğeni Avcısı",
                Description = "Tüm yorumlarında toplam 50+ beğeni al.",
                CurrentValue = toplamBegeni,
                TargetValue = 50,
                Earned = earnedBadges.Any(b => b.BadgeType == "BegeniAvcisi")
            });
            // 5. Sosyal Kelebek
            int coinCesitliligi = comments.Select(c => c.CoinId).Distinct().Count();
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "SosyalKelebek",
                BadgeName = "Sosyal Kelebek",
                Description = "10 farklı coinde yorum yaz.",
                CurrentValue = coinCesitliligi,
                TargetValue = 10,
                Earned = earnedBadges.Any(b => b.BadgeType == "SosyalKelebek")
            });
            // 6. Efsane Yorumcu
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "EfsaneYorumcu",
                BadgeName = "Efsane Yorumcu",
                Description = "Toplam 100 yorum yaz.",
                CurrentValue = comments.Count,
                TargetValue = 100,
                Earned = earnedBadges.Any(b => b.BadgeType == "EfsaneYorumcu")
            });
            // 7. Mega Beğeni Avcısı
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "MegaBegeniAvcisi",
                BadgeName = "Mega Beğeni Avcısı",
                Description = "Tüm yorumlarında toplam 500+ beğeni al.",
                CurrentValue = toplamBegeni,
                TargetValue = 500,
                Earned = earnedBadges.Any(b => b.BadgeType == "MegaBegeniAvcisi")
            });
            // 8. Yanıt Şampiyonu
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "YanitSampiyonu",
                BadgeName = "Yanıt Şampiyonu",
                Description = "100+ yanıt yaz.",
                CurrentValue = replies.Count,
                TargetValue = 100,
                Earned = earnedBadges.Any(b => b.BadgeType == "YanitSampiyonu")
            });
            // 9. Forum Efsanesi
            int forumEfsanesi = comments.GroupBy(c => c.CoinId).Count(g => g.Count() >= 10);
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "ForumEfsanesi",
                BadgeName = "Forum Efsanesi",
                Description = "10 farklı coinde 10'ar yorum yaz.",
                CurrentValue = forumEfsanesi,
                TargetValue = 10,
                Earned = earnedBadges.Any(b => b.BadgeType == "ForumEfsanesi")
            });
            // 10. Süper Star
            int superStar = comments.Any() ? comments.Max(c => c.LikeCount) : 0;
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "SuperStar",
                BadgeName = "Süper Star",
                Description = "Bir yorumun 100+ beğeni alsın.",
                CurrentValue = superStar,
                TargetValue = 100,
                Earned = earnedBadges.Any(b => b.BadgeType == "SuperStar")
            });
            // 11. Yılın Yorumu
            // (Kazanıldıysa göster, ilerleme yok)
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "YilinYorumu",
                BadgeName = "Yılın Yorumu",
                Description = "Son 1 yılın en çok beğeni alan yorumunu yaz.",
                CurrentValue = earnedBadges.Any(b => b.BadgeType == "YilinYorumu") ? 1 : 0,
                TargetValue = 1,
                Earned = earnedBadges.Any(b => b.BadgeType == "YilinYorumu")
            });
            // 12. Sürekli Aktif
            // (Kazanıldıysa göster, ilerleme yok)
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "SurekliAktif",
                BadgeName = "Sürekli Aktif",
                Description = "100 gün boyunca her gün en az 1 yorum yaz.",
                CurrentValue = earnedBadges.Any(b => b.BadgeType == "SurekliAktif") ? 100 : 0,
                TargetValue = 100,
                Earned = earnedBadges.Any(b => b.BadgeType == "SurekliAktif")
            });
            // 13. Topluluk Lideri
            progress.Add(new BadgeProgressViewModel
            {
                BadgeType = "ToplulukLideri",
                BadgeName = "Topluluk Lideri",
                Description = "Tüm yorumlarında toplam 1000+ beğeni al.",
                CurrentValue = toplamBegeni,
                TargetValue = 1000,
                Earned = earnedBadges.Any(b => b.BadgeType == "ToplulukLideri")
            });

            points.Badges = progress;
            // Leaderboard'u çek
            var leaderboardResponse = await _httpClient.GetAsync($"api/UserProfile/leaderboard");
            var leaderboard = new List<LeaderboardEntryViewModel>();
            if (leaderboardResponse.IsSuccessStatusCode)
            {
                var leaderboardJson = await leaderboardResponse.Content.ReadAsStringAsync();
                leaderboard = JsonSerializer.Deserialize<List<LeaderboardEntryViewModel>>(leaderboardJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<LeaderboardEntryViewModel>();
            }
            points.Leaderboard = leaderboard;
            return View(points);
        }
    }
} 