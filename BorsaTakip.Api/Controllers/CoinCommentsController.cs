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

        // Yorum ekleme
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CoinComment comment)
        {
            if (string.IsNullOrEmpty(comment.CoinId) || string.IsNullOrEmpty(comment.Username) || string.IsNullOrEmpty(comment.Comment))
            {
                return BadRequest("Eksik bilgi.");
            }

            comment.CreatedAt = DateTime.UtcNow;

            _context.CoinComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
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
    }
}
