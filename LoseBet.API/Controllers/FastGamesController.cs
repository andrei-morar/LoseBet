using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoseBet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FastGamesController : ControllerBase
    {
        private readonly LoseBetDbContext _context;
        private readonly Random _random;

        public FastGamesController(LoseBetDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        // ==================== JOCUL 1: ALBA-NEAGRA ====================
        [HttpPost("albaneagra")]
        public async Task<IActionResult> PlayAlbaNeagra([FromBody] AlbaNeagraRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("Utilizator negăsit.");

            if (request.BetAmount <= 0) return BadRequest("Pariul trebuie să fie mai mare de 0.");
            if (user.Balance < request.BetAmount) return BadRequest("Fonduri insuficiente!");

            user.Balance -= request.BetAmount;
            _context.Transactions.Add(new Transaction { UserId = user.Id, Amount = request.BetAmount, Type = TransactionType.GameBet, Timestamp = DateTime.Now });

            int winningCup = _random.Next(1, 4);
            bool isWin = (request.ChosenCup == winningCup);
            decimal winAmount = isWin ? request.BetAmount * 3 : 0;

            if (isWin)
            {
                user.Balance += winAmount;
                _context.Transactions.Add(new Transaction { UserId = user.Id, Amount = winAmount, Type = TransactionType.GameWin, Timestamp = DateTime.Now });
            }

            await _context.SaveChangesAsync();
            return Ok(new AlbaNeagraResponse { WinningCup = winningCup, IsWin = isWin, NewBalance = user.Balance, Message = isWin ? $"Câștig {winAmount}!" : "Pierdut." });
        }

        // ==================== JOCUL 2: MINES ====================

        [HttpPost("mines/start")]
        public async Task<IActionResult> StartMines([FromBody] MinesStartRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("User negăsit.");
            if (user.Balance < request.BetAmount) return BadRequest("Fonduri insuficiente.");

            // Închidem sesiunile vechi
            var oldSessions = _context.MinesSessions.Where(s => s.UserId == user.Id && s.IsActive);
            foreach (var s in oldSessions) s.IsActive = false;

            var bombList = new List<int>();
            while (bombList.Count < request.BombCount)
            {
                int pos = _random.Next(0, 25);
                if (!bombList.Contains(pos)) bombList.Add(pos);
            }

            var session = new MinesSession
            {
                UserId = user.Id,
                BetAmount = request.BetAmount,
                BombCount = request.BombCount,
                BombLocations = string.Join(",", bombList),
                IsActive = true,
                CurrentMultiplier = 1.0m
            };

            user.Balance -= request.BetAmount;
            _context.MinesSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { SessionId = session.Id, Message = "Joc pornit!" });
        }

        [HttpPost("mines/pick")]
        public async Task<IActionResult> PickMine([FromBody] MinesPickRequest request)
        {
            var session = await _context.MinesSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId && s.IsActive);
            if (session == null) return NotFound("Sesiune activă negăsită.");

            var bombs = session.BombLocations.Split(',').Select(int.Parse).ToList();
            var revealed = string.IsNullOrEmpty(session.RevealedIndices)
                           ? new List<int>()
                           : session.RevealedIndices.Split(',').Select(int.Parse).ToList();

            if (revealed.Contains(request.Index)) return BadRequest("Căsuță deja deschisă.");

            if (bombs.Contains(request.Index))
            {
                session.IsActive = false;
                session.IsGameOver = true;
                await _context.SaveChangesAsync();
                return Ok(new { HitBomb = true, Bombs = bombs, Message = "BOOM!" });
            }

            revealed.Add(request.Index);
            session.RevealedIndices = string.Join(",", revealed);

            // Matematică pentru multiplicator (realist)
            decimal nextMultiplier = session.CurrentMultiplier * (1.0m + (decimal)session.BombCount / (25 - revealed.Count));
            session.CurrentMultiplier = nextMultiplier;

            await _context.SaveChangesAsync();
            return Ok(new { HitBomb = false, CurrentMultiplier = Math.Round(session.CurrentMultiplier, 2), Message = "Diamant!" });
        }

        [HttpPost("mines/cashout")]
        public async Task<IActionResult> CashOutMines([FromBody] MinesCashOutRequest request)
        {
            var session = await _context.MinesSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId && s.IsActive);
            if (session == null) return BadRequest("Sesiune inactivă.");

            decimal winAmount = session.BetAmount * session.CurrentMultiplier;
            var user = await _context.Users.FindAsync(session.UserId);

            user.Balance += winAmount;
            session.IsActive = false;

            _context.Transactions.Add(new Transaction { UserId = user.Id, Amount = winAmount, Type = TransactionType.GameWin, Timestamp = DateTime.Now });
            await _context.SaveChangesAsync();

            return Ok(new { WinAmount = Math.Round(winAmount, 2), NewBalance = user.Balance });
        }

        // ==================== JOCUL: AVIATOR ====================

        [HttpPost("aviator/start")]
        public async Task<IActionResult> StartAviator([FromBody] AviatorStartRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null || user.Balance < request.BetAmount) return BadRequest("Fonduri insuficiente.");

            // Închidem sesiuni vechi
            var oldSessions = _context.AviatorSessions.Where(s => s.UserId == user.Id && s.IsActive);
            foreach (var s in oldSessions) s.IsActive = false;

            // MATEMATICA AVIATOR: Generăm un Crash Point realist
            // Majoritatea explodează mic, puține ajung la cote mari
            double value = _random.NextDouble();
            decimal crashPoint = (decimal)(0.99 / (1 - value));
            if (crashPoint < 1.0m) crashPoint = 1.0m; // Securanță să nu fie sub 1

            var session = new AviatorSessions
            {
                UserId = user.Id,
                BetAmount = request.BetAmount,
                CrashPoint = Math.Round(crashPoint, 2),
                IsActive = true,
                StartTime = DateTime.Now
            };

            user.Balance -= request.BetAmount;
            _context.AviatorSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                SessionId = session.Id,
                Message = "Avionul a decolat!",
                CrashPoint = session.CrashPoint // <-- ADAUGĂ ASTA
            });
        }

        [HttpPost("aviator/cashout")]
        public async Task<IActionResult> CashOutAviator([FromBody] AviatorCashOutRequest request)
        {
            var session = await _context.AviatorSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId && s.IsActive);
            if (session == null) return BadRequest("Sesiune inactivă.");

            // Verificăm dacă multiplicatorul cerut de user este sub CrashPoint
            if (request.ClaimedMultiplier > session.CrashPoint)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { Success = false, Message = $"FLY AWAY! Avionul a explodat la {session.CrashPoint}x" });
            }

            // Dacă e ok, plătim
            var user = await _context.Users.FindAsync(session.UserId);
            decimal winAmount = session.BetAmount * request.ClaimedMultiplier;

            user.Balance += winAmount;
            session.IsActive = false;

            _context.Transactions.Add(new Transaction { UserId = user.Id, Amount = winAmount, Type = TransactionType.GameWin, Timestamp = DateTime.Now });
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, WinAmount = winAmount, Message = $"Cash Out reușit la {request.ClaimedMultiplier}x!" });
        }


    } // <--- CLASA SE ÎNCHIDE AICI

    // ==================== DTO-uri ====================
    public class AlbaNeagraRequest { public int UserId { get; set; } public decimal BetAmount { get; set; } public int ChosenCup { get; set; } }
    public class AlbaNeagraResponse { public int WinningCup { get; set; } public bool IsWin { get; set; } public decimal NewBalance { get; set; } public string Message { get; set; } = string.Empty; }
    public class MinesStartRequest { public int UserId { get; set; } public decimal BetAmount { get; set; } public int BombCount { get; set; } }
    public class MinesPickRequest { public int SessionId { get; set; } public int Index { get; set; } }
    public class MinesCashOutRequest { public int SessionId { get; set; } }
    // DTOs pentru Aviator
    public class AviatorStartRequest { public int UserId { get; set; } public decimal BetAmount { get; set; } }
    public class AviatorCashOutRequest { public int SessionId { get; set; } public decimal ClaimedMultiplier { get; set; } }
}