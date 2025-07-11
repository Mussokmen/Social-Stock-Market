using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorsaTakip.Api.Data;
using BorsaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BorsaTakip.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentRepliesController : ControllerBase
    {
        private readonly BorsaTakipDbContext _context;

        public CommentRepliesController(BorsaTakipDbContext context)
        {
            _context = context;
        }

        // Yeni yanıt ekle
        [HttpPost]
        public async Task<IActionResult> AddReply([FromBody] CommentReply reply)
        {
            if (string.IsNullOrEmpty(reply.Comment) || string.IsNullOrEmpty(reply.Username) || reply.CoinCommentId <= 0)
            {
                return BadRequest("Eksik bilgi.");
            }

            reply.CreatedAt = DateTime.UtcNow;

            _context.CommentReplies.Add(reply);
            await _context.SaveChangesAsync();

            return Ok(reply);
        }

        // Belirli bir yoruma ait yanıtları getir
        [HttpGet("bycomment/{coinCommentId}")]
        public async Task<IActionResult> GetRepliesByComment(int coinCommentId)
        {
            var replies = await _context.CommentReplies
                .Where(r => r.CoinCommentId == coinCommentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(replies);
        }
    }
}


