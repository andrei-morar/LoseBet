using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data; // Asigură-te că namespace-ul acesta este corect pentru contextul tău
using LoseBet.Core.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoseBet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly LoseBetDbContext _context;
        private readonly Random _random;

        public SlotsController(LoseBetDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        [HttpPost("slots")]
        public async Task<IActionResult> PlaySlots([FromBody] SlotsRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("Utilizator negăsit.");
            if (user.Balance < request.BetAmount) return BadRequest("Fonduri insuficiente.");

            // Luăm banii înainte
            user.Balance -= request.BetAmount;
            _context.Transactions.Add(new Transaction { UserId = user.Id, Amount = request.BetAmount, Type = TransactionType.GameBet, Timestamp = DateTime.Now });

            // 1. Configurare temă și Wild
            string[] symbols;
            string wildSymbol = "";
            bool canExpand = false;

            switch (request.Theme.ToLower())
            {
                case "shiningcrown":
                    symbols = new[] { "🍒", "🍋", "🍊", "🔔", "🍉", "7️⃣", "👑" };
                    wildSymbol = "👑"; canExpand = true;
                    break;
                case "burninghot":
                    symbols = new[] { "🍒", "🍋", "🍊", "🔔", "⭐", "7️⃣", "🍀" };
                    wildSymbol = "🍀"; canExpand = true;
                    break;
                case "diceroll":
                    symbols = new[] { "🍒", "🍋", "🍊", "🔔", "⭐", "7️⃣", "🎲" };
                    wildSymbol = "🎲"; canExpand = true;
                    break;
                default:
                    symbols = new[] { "🍒", "🍋", "🍊", "🍉", "7️⃣" };
                    break;
            }

            // 2. Generăm grila inițială și salvăm unde pică Wild-urile (SĂ NU UITĂM SĂ LE ADĂUGĂM!)
            string[][] grid = new string[request.Rows][];
            var initialWildDrops = new List<dynamic>();

            for (int r = 0; r < request.Rows; r++)
            {
                grid[r] = new string[request.Columns];
                for (int c = 0; c < request.Columns; c++)
                {
                    grid[r][c] = symbols[_random.Next(symbols.Length)];
                    if (grid[r][c] == wildSymbol && canExpand)
                    {
                        initialWildDrops.Add(new { row = r, col = c });
                    }
                }
            }

            // 3. LOGICA DE REALISM: Expanding Wilds
            var expandedColumns = new List<int>();
            if (canExpand && initialWildDrops.Count > 0)
            {
                foreach (var pos in initialWildDrops)
                {
                    int r = pos.row;
                    int c = pos.col;

                    if (request.Theme.ToLower() == "diceroll")
                    {
                        // Explozie 3x3
                        for (int dr = -1; dr <= 1; dr++)
                        {
                            for (int dc = -1; dc <= 1; dc++)
                            {
                                int nR = r + dr; int nC = c + dc;
                                if (nR >= 0 && nR < request.Rows && nC >= 0 && nC < request.Columns)
                                    grid[nR][nC] = wildSymbol;
                            }
                        }
                    }
                    else
                    {
                        // Extindere pe coloană (EGT style)
                        if (!expandedColumns.Contains(c)) expandedColumns.Add(c);
                        for (int rowIdx = 0; rowIdx < request.Rows; rowIdx++)
                        {
                            grid[rowIdx][c] = wildSymbol;
                        }
                    }
                }
            }

            // 4. Verificăm câștigurile
            decimal totalWin = 0;
            var winningLines = new List<string>();

            for (int r = 0; r < request.Rows; r++)
            {
                string matchSymbol = "";
                int matchCount = 0;

                for (int c = 0; c < request.Columns; c++)
                {
                    string current = grid[r][c];
                    if (matchSymbol == "") { if (current != wildSymbol) matchSymbol = current; matchCount++; }
                    else if (current == matchSymbol || current == wildSymbol) matchCount++;
                    else break;
                }

                if (matchSymbol == "") matchSymbol = wildSymbol;
                if (matchCount >= 3)
                {
                    decimal lineWin = request.BetAmount * GetMultiplierForSlotSymbol(matchSymbol) * (matchCount - 2);
                    totalWin += lineWin;
                    winningLines.Add($"Linia {r + 1}: {matchCount}x {matchSymbol}");
                }
            }

            if (totalWin > 0)
            {
                user.Balance += totalWin;
                _context.Transactions.Add(new Transaction { UserId = user.Id, Amount = totalWin, Type = TransactionType.GameWin, Timestamp = DateTime.Now });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Grid = grid,
                AnimationData = new
                {
                    WildSymbol = wildSymbol,
                    ExpandedColumns = expandedColumns,
                    InitialWildDrops = initialWildDrops
                },
                IsWin = totalWin > 0,
                TotalWin = totalWin,
                WinningLines = winningLines,
                NewBalance = user.Balance
            });
        }

        private decimal GetMultiplierForSlotSymbol(string symbol)
        {
            return symbol switch
            {
                "🍒" => 2m,
                "🍋" => 2m,
                "🍊" => 3m,
                "🍉" => 5m,
                "🔔" => 5m,
                "🍀" => 10m,
                "👑" => 20m,
                "⭐" => 25m,
                "🎲" => 15m,
                "7️⃣" => 50m,
                _ => 1m
            };
        }
    }

    // DTO-ul poate sta aici sau într-un fișier separat
    public class SlotsRequest
    {
        public int UserId { get; set; }
        public decimal BetAmount { get; set; }
        public int Rows { get; set; } = 3;
        public int Columns { get; set; } = 5;
        public string Theme { get; set; } = "ShiningCrown";
    }
}