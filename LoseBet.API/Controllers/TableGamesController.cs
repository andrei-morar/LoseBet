using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data; // Asigură-te că namespace-ul acesta corespunde proiectului tău
using LoseBet.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoseBet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableGamesController : ControllerBase
    {
        private readonly LoseBetDbContext _context;
        private readonly Random _random;

        // Constructorul care "injectează" baza de date și pornește RNG-ul
        public TableGamesController(LoseBetDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        // ==================== JOCUL: RULETĂ AMERICANĂ (Cu 0 și 00) ====================
        [HttpPost("roulette")]
        public async Task<IActionResult> PlayRoulette([FromBody] RouletteRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("Utilizator negăsit.");
            if (user.Balance < request.BetAmount) return BadRequest("Fonduri insuficiente.");

            user.Balance -= request.BetAmount;
            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                Amount = request.BetAmount,
                Type = TransactionType.GameBet,
                Timestamp = DateTime.Now
            });

            // 1. Generăm rezultatul (0-37, unde 37 este "00")
            int rngIndex = _random.Next(0, 38);
            string resultNumberStr = rngIndex == 37 ? "00" : rngIndex.ToString();

            bool isWin = false;
            decimal multiplier = 0;

            switch (request.BetType.ToLower())
            {
                case "number":
                    if (request.BetValue == resultNumberStr)
                    {
                        isWin = true;
                        multiplier = 36;
                    }
                    break;

                case "color":
                    string resultColor = GetColorForNumberStr(resultNumberStr);
                    if (request.BetValue.Equals(resultColor, StringComparison.OrdinalIgnoreCase))
                    {
                        isWin = true;
                        multiplier = 2;
                    }
                    break;

                case "parity":
                    if (resultNumberStr != "0" && resultNumberStr != "00")
                    {
                        int actualNumber = int.Parse(resultNumberStr);
                        bool isEven = actualNumber % 2 == 0;
                        string parityValue = isEven ? "even" : "odd";
                        if (request.BetValue.ToLower() == parityValue)
                        {
                            isWin = true;
                            multiplier = 2;
                        }
                    }
                    break;
            }

            decimal winAmount = isWin ? request.BetAmount * multiplier : 0;
            if (isWin)
            {
                user.Balance += winAmount;
                _context.Transactions.Add(new Transaction
                {
                    UserId = user.Id,
                    Amount = winAmount,
                    Type = TransactionType.GameWin,
                    Timestamp = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Number = resultNumberStr,
                Color = GetColorForNumberStr(resultNumberStr),
                IsWin = isWin,
                Payout = winAmount,
                NewBalance = user.Balance,
                Message = isWin ? $"Felicitări! A ieșit {resultNumberStr}. Ai câștigat {winAmount} RON!" : $"A ieșit {resultNumberStr}. Mai încearcă!"
            });
        }

        private string GetColorForNumberStr(string numberStr)
        {
            if (numberStr == "0" || numberStr == "00") return "green";

            int number = int.Parse(numberStr);
            int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            return redNumbers.Contains(number) ? "red" : "black";
        }

        // ==================== JOCUL: BLACKJACK ====================

        [HttpPost("blackjack/deal")]
        public async Task<IActionResult> DealBlackjack([FromBody] BlackjackStartRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null || user.Balance < request.BetAmount) return BadRequest("Fonduri insuficiente.");

            // Închidem sesiuni vechi
            var oldSessions = _context.BlackjackSessions.Where(s => s.UserId == user.Id && s.IsActive);
            foreach (var s in oldSessions) s.IsActive = false;

            // Generăm cărțile inițiale (valori 2-11)
            var userCards = new List<int> { GenerateCard(), GenerateCard() };
            var dealerCards = new List<int> { GenerateCard(), GenerateCard() };

            var session = new BlackjackSession
            {
                UserId = user.Id,
                BetAmount = request.BetAmount,
                UserHand = string.Join(",", userCards),
                DealerHand = string.Join(",", dealerCards),
                IsActive = true
            };

            user.Balance -= request.BetAmount;
            _context.BlackjackSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                SessionId = session.Id,
                YourCards = userCards,
                DealerVisibleCard = dealerCards[0], // Vedem doar prima carte a dealerului
                YourTotal = CalculateScore(userCards)
            });
        }

        [HttpPost("blackjack/hit")]
        public async Task<IActionResult> HitBlackjack([FromBody] BlackjackActionRequest request)
        {
            var session = await _context.BlackjackSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId && s.IsActive);
            if (session == null) return NotFound("Joc negăsit.");

            var userCards = session.UserHand.Split(',').Select(int.Parse).ToList();
            userCards.Add(GenerateCard());
            session.UserHand = string.Join(",", userCards);

            int score = CalculateScore(userCards);
            if (score > 21)
            {
                session.IsActive = false;
                session.Status = "Bust";
                await _context.SaveChangesAsync();
                return Ok(new { Cards = userCards, Score = score, Status = "Bust! Ai depășit 21." });
            }

            await _context.SaveChangesAsync();
            return Ok(new { Cards = userCards, Score = score, Status = "In Progress" });
        }

        [HttpPost("blackjack/stand")]
        public async Task<IActionResult> StandBlackjack([FromBody] BlackjackActionRequest request)
        {
            var session = await _context.BlackjackSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId && s.IsActive);
            if (session == null) return NotFound("Sesiune activă negăsită.");

            var userCards = session.UserHand.Split(',').Select(int.Parse).ToList();
            var dealerCards = session.DealerHand.Split(',').Select(int.Parse).ToList();

            int userScore = CalculateScore(userCards);
            int dealerScore = CalculateScore(dealerCards);

            // DEALER LOGIC: Trebuie să tragă până la minim 17 (Soft 17 rule)
            while (dealerScore < 17)
            {
                dealerCards.Add(GenerateCard());
                dealerScore = CalculateScore(dealerCards);
            }

            session.DealerHand = string.Join(",", dealerCards);
            session.IsActive = false;

            // Determinăm câștigătorul
            if (dealerScore > 21) session.Status = "Win";
            else if (userScore > dealerScore) session.Status = "Win";
            else if (userScore < dealerScore) session.Status = "Lose";
            else session.Status = "Draw";

            // Procesăm plata
            if (session.Status == "Win")
            {
                var user = await _context.Users.FindAsync(session.UserId);
                decimal payout = session.BetAmount * 2;
                user.Balance += payout;
                _context.Transactions.Add(new Transaction { UserId = user.Id, Amount = payout, Type = TransactionType.GameWin, Timestamp = DateTime.Now });
            }
            else if (session.Status == "Draw")
            {
                var user = await _context.Users.FindAsync(session.UserId);
                user.Balance += session.BetAmount;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                DealerCards = dealerCards,
                DealerTotal = dealerScore,
                UserTotal = userScore,
                Result = session.Status,
                Message = GetBlackjackMessage(session.Status, dealerScore)
            });
        }

        // Funcții helper pentru Blackjack
        private int GenerateCard()
        {
            // Generăm un număr de la 1 la 13 (ca într-un pachet de cărți)
            // 1 = As, 11 = J, 12 = Q, 13 = K
            int card = _random.Next(1, 14);

            if (card == 1) return 11; // Îl tratăm inițial ca 11
            if (card > 10) return 10; // J, Q, K valorează 10
            return card;
        }
        private int CalculateScore(List<int> cards)
        {
            int score = cards.Sum();
            int acesCount = cards.Count(c => c == 11);

            // Cât timp scorul e peste 21 și avem Ași care valorează 11
            while (score > 21 && acesCount > 0)
            {
                score -= 10; // Transformăm un As din 11 în 1
                acesCount--;
            }

            return score;
        }

        private string GetBlackjackMessage(string status, int dealerScore)
        {
            return status switch
            {
                "Win" => dealerScore > 21 ? "Dealer Bust! Ai câștigat!" : "Ai bătut dealerul! Felicitări!",
                "Lose" => "Ai pierdut. Casa câștigă.",
                "Draw" => "Egalitate. Ai primit banii înapoi.",
                _ => ""
            };
        }

    }

    // DTO-ul pus în afara clasei controller, dar în interiorul namespace-ului
    public class RouletteRequest
    {
        public int UserId { get; set; }
        public decimal BetAmount { get; set; }
        public string BetType { get; set; } = string.Empty;
        public string BetValue { get; set; } = string.Empty;
    }

    public class BlackjackStartRequest { 
        public int UserId { get; set; } 
        public decimal BetAmount { get; set; } 
    }

    public class BlackjackActionRequest
    {
        public int SessionId { get; set; }
    }

}

