using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;

namespace LoseBet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly LoseBetDbContext _context;

        public WalletController(LoseBetDbContext context)
        {
            _context = context;
        }

        // 1. Vezi câți bani ai
        [HttpGet("balance/{userId}")]
        public async Task<IActionResult> GetBalance(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Utilizator negăsit.");

            return Ok(new { Balance = user.Balance });
        }

        // 2. Depunere de bani
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] WalletRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("Utilizator negăsit.");

            if (request.Amount <= 0) return BadRequest("Suma trebuie să fie pozitivă.");

            // Actualizăm soldul
            user.Balance += request.Amount;

            // Înregistrăm tranzacția în istoric
            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = request.Amount,
                Type = TransactionType.Deposit,
                Timestamp = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok($"Depunere reușită! Sold nou: {user.Balance} RON");
        }

        // 3. Retragere de bani
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WalletRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("Utilizator negăsit.");

            if (request.Amount <= 0) return BadRequest("Suma invalidă.");
            if (user.Balance < request.Amount) return BadRequest("Fonduri insuficiente!");

            // Scădem banii
            user.Balance -= request.Amount;

            // Înregistrăm tranzacția
            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = request.Amount,
                Type = TransactionType.Withdrawal,
                Timestamp = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok($"Retragere reușită! Sold rămas: {user.Balance} RON");
        }
    }

    // Modelul de date pentru cererile de bani
    public class WalletRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}