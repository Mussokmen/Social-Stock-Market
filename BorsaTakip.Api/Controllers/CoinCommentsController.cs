using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorsaTakip.Api.Data;
using BorsaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BorsaTakip.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinCommentsController : ControllerBase
    {
        private readonly BorsaTakipDbContext _context;

        public CoinCommentsController(BorsaTakipDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CoinComment comment)
        {
            if (comment == null)
                return BadRequest("Yorum verisi boş olamaz.");

            if (string.IsNullOrWhiteSpace(comment.CoinId) ||
                string.IsNullOrWhiteSpace(comment.Username) ||
                string.IsNullOrWhiteSpace(comment.Comment))
            {
                return BadRequest("CoinId, Username ve Comment alanları zorunludur.");
            }

            comment.CreatedAt = DateTime.UtcNow;
            
           

            _context.CoinComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                comment.Id,
                comment.Username,
                comment.CoinId,
                comment.Comment,
                comment.CreatedAt,
               
            });
        }

        // Belirli coin yorumlarını listele
        [HttpGet("{coinId}")]
        public async Task<IActionResult> GetComments(string coinId)
        {
            var comments = await _context.CoinComments
                .Where(c => c.CoinId == coinId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        // Tek bir yorumu getiren GET endpoint’i
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetComment(int id)
        {
            var comment = await _context.CoinComments.FindAsync(id);

            if (comment == null)
                return NotFound(new { message = "Yorum bulunamadı." });

            return Ok(comment);
        }

        // Yorum güncelleme için PUT endpoint’i
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CoinComment updatedComment)
        {
            if (id != updatedComment.Id)
                return BadRequest(new { message = "ID uyuşmuyor." });

            var existingComment = await _context.CoinComments.FindAsync(id);
            if (existingComment == null)
                return NotFound(new { message = "Yorum bulunamadı." });

            existingComment.Comment = updatedComment.Comment;
            existingComment.CoinId = updatedComment.CoinId;
            // Username ve CreatedAt değiştirilmez.

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // Başarılı güncelleme
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Güncelleme sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.CoinComments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { message = "Yorum bulunamadı." });
            }

            _context.CoinComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yorum başarıyla silindi." });
        }

        // Tüm yorumları listeler
        [HttpGet("all")]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await _context.CoinComments
                .ToListAsync();

            return Ok(comments);
        }

        // Kullanıcının yaptığı yorumları getir
        [HttpGet("usercomments/{username}")]
        public async Task<IActionResult> GetUserComments(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Kullanıcı adı gerekli.");

            var comments = await _context.CoinComments
                .Where(c => c.Username == username)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }
        [HttpPost("{commentId}/like")]
        public async Task<IActionResult> LikeComment(int commentId, [FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Kullanıcı adı gerekli.");

            // UserId'yi username'den bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");
            var userId = user.Id;

            var comment = await _context.CoinComments.FindAsync(commentId);
            if (comment == null)
                return NotFound();

            var alreadyLiked = await _context.CommentLikes.AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
            if (alreadyLiked)
                return BadRequest("Zaten beğendiniz.");

            var like = new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommentLikes.Add(like);
            comment.LikeCount++;
            await _context.SaveChangesAsync();

            return Ok(new { Likes = comment.LikeCount });
        }






    }
}
