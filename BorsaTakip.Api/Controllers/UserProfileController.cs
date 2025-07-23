using System;
using System.IO;
using System.Threading.Tasks;
using BorsaTakip.Api.Data;       // DbContext namespace'i
using BorsaTakip.Api.Models;     // ApplicationUser modeli burada olabilir
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Where, Select, Distinct, Sum

namespace BorsaTakip.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly BorsaTakipDbContext _context;

        // Constructor injection ile context al
        public UserProfileController(BorsaTakipDbContext context)
        {
            _context = context;
        }

        // GET api/UserProfile/bio/{username}
        [HttpGet("bio/{username}")]
        public async Task<IActionResult> GetBio(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username boş olamaz.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            return Ok(user.Bio ?? "");
        }

        // POST api/UserProfile/updatebio
        [HttpPost("updatebio")]
        public async Task<IActionResult> UpdateBio([FromBody] UpdateBioRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("Geçersiz kullanıcı.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.Username);
            if (user == null)
                return NotFound();

            user.Bio = request.Bio ?? "";
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST api/UserProfile/uploadphoto
        [HttpPost("uploadphoto")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo, [FromForm] string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Username boş olamaz.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (photo == null || photo.Length == 0)
                return BadRequest("Fotoğraf yüklenmedi.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "ProfilePhotos");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{username}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Kaydedilen fotoğraf yolunu DB'de sakla
            user.PhotoUrl = $"/Uploads/ProfilePhotos/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { PhotoUrl = user.PhotoUrl });
        }

        // GET api/UserProfile/badges/{username}
        [HttpGet("badges/{username}")]
        public async Task<IActionResult> GetBadges(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username boş olamaz.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");
            var userId = user.Id;

            var badges = await _context.UserBadges.Where(b => b.UserId == userId).ToListAsync();
            var badgeTypes = badges.Select(b => b.BadgeType).ToHashSet();
            var now = DateTime.UtcNow;
            var newBadges = new List<string>();

            // 1. Popüler Yorumcu: 10+ beğeni alan yorumu olan
            var populerYorumcu = await _context.CoinComments.AnyAsync(c => c.Username == username && c.LikeCount >= 10);
            if (populerYorumcu && !badgeTypes.Contains("PopulerYorumcu"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "PopulerYorumcu", EarnedAt = now });
                newBadges.Add("PopulerYorumcu");
            }
            // 2. Sürekli Yorumcu: 20+ yorum yapan
            var yorumSayisi = await _context.CoinComments.CountAsync(c => c.Username == username);
            if (yorumSayisi >= 20 && !badgeTypes.Contains("SurekliYorumcu"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "SurekliYorumcu", EarnedAt = now });
                newBadges.Add("SurekliYorumcu");
            }
            // 3. Cevap Ustası: 10+ yanıt yazan
            var yanitSayisi = await _context.CommentReplies.CountAsync(r => r.Username == username);
            if (yanitSayisi >= 10 && !badgeTypes.Contains("CevapUstasi"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "CevapUstasi", EarnedAt = now });
                newBadges.Add("CevapUstasi");
            }
            // 4. Beğeni Avcısı: toplam 50+ beğeni toplayan
            var toplamBegeni = await _context.CoinComments.Where(c => c.Username == username).SumAsync(c => (int?)c.LikeCount) ?? 0;
            if (toplamBegeni >= 50 && !badgeTypes.Contains("BegeniAvcisi"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "BegeniAvcisi", EarnedAt = now });
                newBadges.Add("BegeniAvcisi");
            }
            // 5. Sosyal Kelebek: 10 farklı coinde yorum yapan
            var coinCesitliligi = await _context.CoinComments.Where(c => c.Username == username).Select(c => c.CoinId).Distinct().CountAsync();
            if (coinCesitliligi >= 10 && !badgeTypes.Contains("SosyalKelebek"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "SosyalKelebek", EarnedAt = now });
                newBadges.Add("SosyalKelebek");
            }
            // Efsane Yorumcu: 100+ yorum
            if (yorumSayisi >= 100 && !badgeTypes.Contains("EfsaneYorumcu"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "EfsaneYorumcu", EarnedAt = now });
                newBadges.Add("EfsaneYorumcu");
            }
            // Mega Beğeni Avcısı: toplam 500+ beğeni
            if (toplamBegeni >= 500 && !badgeTypes.Contains("MegaBegeniAvcisi"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "MegaBegeniAvcisi", EarnedAt = now });
                newBadges.Add("MegaBegeniAvcisi");
            }
            // Yanıt Şampiyonu: 100+ yanıt
            if (yanitSayisi >= 100 && !badgeTypes.Contains("YanitSampiyonu"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "YanitSampiyonu", EarnedAt = now });
                newBadges.Add("YanitSampiyonu");
            }
            // Forum Efsanesi: 10 farklı coinde 10'ar yorum
            var coin10Yorum = await _context.CoinComments
                .Where(c => c.Username == username)
                .GroupBy(c => c.CoinId)
                .CountAsync(g => g.Count() >= 10);
            if (coin10Yorum >= 10 && !badgeTypes.Contains("ForumEfsanesi"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "ForumEfsanesi", EarnedAt = now });
                newBadges.Add("ForumEfsanesi");
            }
            // Süper Star: bir yorumu 100+ beğeni alan
            var superStar = await _context.CoinComments.AnyAsync(c => c.Username == username && c.LikeCount >= 100);
            if (superStar && !badgeTypes.Contains("SuperStar"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "SuperStar", EarnedAt = now });
                newBadges.Add("SuperStar");
            }
            // Yılın Yorumu: son 1 yılın en çok beğeni alan yorumu (dinamik)
            var oneYearAgo = now.AddYears(-1);
            var yilYorumu = await _context.CoinComments
                .Where(c => c.CreatedAt >= oneYearAgo)
                .OrderByDescending(c => c.LikeCount)
                .FirstOrDefaultAsync();
            if (yilYorumu != null && yilYorumu.Username == username && !badgeTypes.Contains("YilinYorumu"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "YilinYorumu", EarnedAt = now });
                newBadges.Add("YilinYorumu");
            }
            // Sürekli Aktif: 100 gün boyunca her gün en az 1 yorum
            var last100Days = Enumerable.Range(0, 100)
                .Select(offset => now.Date.AddDays(-offset))
                .ToList();
            var aktifGunler = await _context.CoinComments
                .Where(c => c.Username == username && c.CreatedAt >= now.AddDays(-100))
                .Select(c => c.CreatedAt.Date)
                .Distinct()
                .ToListAsync();
            if (last100Days.All(d => aktifGunler.Contains(d)) && !badgeTypes.Contains("SurekliAktif"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "SurekliAktif", EarnedAt = now });
                newBadges.Add("SurekliAktif");
            }
            // Topluluk Lideri: toplam 1000+ beğeni
            if (toplamBegeni >= 1000 && !badgeTypes.Contains("ToplulukLideri"))
            {
                _context.UserBadges.Add(new Models.UserBadge { UserId = userId, BadgeType = "ToplulukLideri", EarnedAt = now });
                newBadges.Add("ToplulukLideri");
            }
            if (newBadges.Count > 0)
                await _context.SaveChangesAsync();

            // Son rozet listesini tekrar çek
            var allBadges = await _context.UserBadges.Where(b => b.UserId == userId).ToListAsync();
            return Ok(allBadges.Select(b => new { b.BadgeType, b.EarnedAt }));
        }

        // GET api/UserProfile/points/{username}
        [HttpGet("points/{username}")]
        public async Task<IActionResult> GetUserPoints(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username boş olamaz.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var usernameLower = username.ToLower();
            // Yorumlar
            var comments = await _context.CoinComments.Where(c => c.Username.ToLower() == usernameLower).ToListAsync();
            int yorumPuan = comments.Count * 2;
            // Beğeni (alınan)
            int begeniPuan = comments.Sum(c => c.LikeCount) * 5;
            // Yanıtlar
            int yanitPuan = await _context.CommentReplies.CountAsync(r => r.Username.ToLower() == usernameLower);
            // Rozetler
            int rozetPuan = await _context.UserBadges.CountAsync(b => b.UserId == user.Id) * 20;
            // Her gün ilk yorum bonusu
            var gunler = comments.Select(c => c.CreatedAt.Date).Distinct().Count();
            int gunlukBonus = gunler * 3;

            int toplamPuan = yorumPuan + begeniPuan + yanitPuan + rozetPuan + gunlukBonus;

            return Ok(new { TotalPoints = toplamPuan, YorumPuan = yorumPuan, BegeniPuan = begeniPuan, YanitPuan = yanitPuan, RozetPuan = rozetPuan, GunlukBonus = gunlukBonus });
        }

        // GET api/UserProfile/leaderboard
        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard()
        {
            var users = await _context.Users.ToListAsync();
            var leaderboard = new List<object>();
            foreach (var user in users)
            {
                var username = user.UserName;
                var usernameLower = username.ToLower();
                var comments = await _context.CoinComments.Where(c => c.Username.ToLower() == usernameLower).ToListAsync();
                int yorumPuan = comments.Count * 2;
                int begeniPuan = comments.Sum(c => c.LikeCount) * 5;
                int yanitPuan = await _context.CommentReplies.CountAsync(r => r.Username.ToLower() == usernameLower);
                int rozetPuan = await _context.UserBadges.CountAsync(b => b.UserId == user.Id) * 20;
                var gunler = comments.Select(c => c.CreatedAt.Date).Distinct().Count();
                int gunlukBonus = gunler * 3;
                int toplamPuan = yorumPuan + begeniPuan + yanitPuan + rozetPuan + gunlukBonus;
                leaderboard.Add(new { Username = username, TotalPoints = toplamPuan });
            }
            var sorted = leaderboard.OrderByDescending(x => ((dynamic)x).TotalPoints).Take(20).ToList();
            return Ok(sorted);
        }

        // POST api/UserProfile/follow
        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] FollowRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FollowerUsername) || string.IsNullOrWhiteSpace(req.FollowedUsername))
                return BadRequest("Kullanıcı adı eksik.");
            if (req.FollowerUsername == req.FollowedUsername)
                return BadRequest("Kendini takip edemezsin.");
            var follower = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == req.FollowerUsername.ToLower());
            var followed = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == req.FollowedUsername.ToLower());
            if (follower == null || followed == null)
                return NotFound("Kullanıcı bulunamadı.");
            bool already = await _context.UserFollows.AnyAsync(f => f.FollowerId == follower.Id && f.FollowedId == followed.Id);
            if (already)
                return BadRequest("Zaten takip ediyorsun.");
            _context.UserFollows.Add(new Models.UserFollow { FollowerId = follower.Id, FollowedId = followed.Id, FollowedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return Ok();
        }
        // DELETE api/UserProfile/unfollow
        [HttpPost("unfollow")]
        public async Task<IActionResult> Unfollow([FromBody] FollowRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FollowerUsername) || string.IsNullOrWhiteSpace(req.FollowedUsername))
                return BadRequest("Kullanıcı adı eksik.");
            var follower = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == req.FollowerUsername.ToLower());
            var followed = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == req.FollowedUsername.ToLower());
            if (follower == null || followed == null)
                return NotFound("Kullanıcı bulunamadı.");
            var follow = await _context.UserFollows.FirstOrDefaultAsync(f => f.FollowerId == follower.Id && f.FollowedId == followed.Id);
            if (follow == null)
                return BadRequest("Takip etmiyorsun.");
            _context.UserFollows.Remove(follow);
            await _context.SaveChangesAsync();
            return Ok();
        }
        // GET api/UserProfile/followers/{username}
        [HttpGet("followers/{username}")]
        public async Task<IActionResult> GetFollowers(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null) return NotFound();
            var followers = await _context.UserFollows.Where(f => f.FollowedId == user.Id).ToListAsync();
            var followerUsers = await _context.Users.Where(u => followers.Select(f => f.FollowerId).Contains(u.Id)).Select(u => u.UserName).ToListAsync();
            return Ok(new { Count = followerUsers.Count, Users = followerUsers });
        }
        // GET api/UserProfile/following/{username}
        [HttpGet("following/{username}")]
        public async Task<IActionResult> GetFollowing(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null) return NotFound();
            var following = await _context.UserFollows.Where(f => f.FollowerId == user.Id).ToListAsync();
            var followingUsers = await _context.Users.Where(u => following.Select(f => f.FollowedId).Contains(u.Id)).Select(u => u.UserName).ToListAsync();
            return Ok(new { Count = followingUsers.Count, Users = followingUsers });
        }
        public class FollowRequest
        {
            public string FollowerUsername { get; set; }
            public string FollowedUsername { get; set; }
        }
    }

    public class UpdateBioRequest
    {
        public string Username { get; set; }
        public string Bio { get; set; }
    }
}
