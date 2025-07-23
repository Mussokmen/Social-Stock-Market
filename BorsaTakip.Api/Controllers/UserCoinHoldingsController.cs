using Microsoft.AspNetCore.Mvc;
using BorsaTakip.Api.Data;
using BorsaTakip.Api.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BorsaTakip.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserCoinHoldingsController : ControllerBase
    {
        private readonly BorsaTakipDbContext _context;
        public UserCoinHoldingsController(BorsaTakipDbContext context)
        {
            _context = context;
        }

        // GET: api/UserCoinHoldings/{username}
        [HttpGet("{username}")]
        public async Task<ActionResult<IEnumerable<UserCoinHolding>>> GetUserHoldings(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Kullanıcı adı gerekli.");

            var holdings = await _context.UserCoinHoldings
                .Where(x => x.Username == username)
                .OrderByDescending(x => x.BuyDate)
                .ToListAsync();
            return Ok(holdings);
        }

        // POST: api/UserCoinHoldings
        [HttpPost]
        public async Task<ActionResult<UserCoinHolding>> AddHolding([FromBody] UserCoinHolding holding)
        {
            if (holding == null || string.IsNullOrWhiteSpace(holding.Username) || string.IsNullOrWhiteSpace(holding.CoinName))
                return BadRequest("Eksik bilgi.");

            holding.BuyDate = holding.BuyDate == default ? System.DateTime.UtcNow : holding.BuyDate;
            _context.UserCoinHoldings.Add(holding);
            await _context.SaveChangesAsync();
            return Ok(holding);
        }

        // DELETE: api/UserCoinHoldings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHolding(int id)
        {
            var holding = await _context.UserCoinHoldings.FindAsync(id);
            if (holding == null)
                return NotFound();
            _context.UserCoinHoldings.Remove(holding);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 