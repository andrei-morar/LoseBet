using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;

namespace LoseBet.API.Controllers
{

    public class SymbolConfigDTO
    {
        public string ImageName { get; set; } = string.Empty;
        public decimal Multiplier3 { get; set; }
        public decimal Multiplier4 { get; set; }
        public decimal Multiplier5 { get; set; }
        public bool IsWild { get; set; }
    }

    public class AddSlotRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public string ThemeColor { get; set; } = "";
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Paylines { get; set; }
        public string WildType { get; set; } = "";
        public bool IsActive { get; set; }

        // Acum nu mai primim doar string-uri, ci simboluri complete cu plăți!
        public List<SymbolConfigDTO> Symbols { get; set; } = new();
    }

    // Asigură-te că SpinRequest-ul folosește UserId, nu Username
    public class SlotSpinRequest
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal BetAmount { get; set; }
    }

    public class SlotSpinResponse
    {
        public string[][] Grid { get; set; }
        public decimal NewBalance { get; set; }
        public decimal TotalWin { get; set; }
        public bool IsWin { get; set; }
        public string Message { get; set; }
    }

    public class GambleRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Guess { get; set; } // Va primi "Red" sau "Black"
    }

    public class GambleResponse
    {
        public bool IsWin { get; set; }
        public string CardColor { get; set; }
        public decimal NewBalance { get; set; }
        public decimal WinAmount { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly LoseBetDbContext _context;
        private readonly Random _random = new Random();

        public SlotsController(LoseBetDbContext context) { _context = context; }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSlots() => Ok(await _context.SlotGames.Include(g => g.Symbols).ToListAsync());

        [HttpPost("add")]
        public async Task<IActionResult> AddSlotGame([FromBody] AddSlotRequest req)
        {
            var game = new SlotGame
            {
                Name = req.Name,
                ThumbnailUrl = req.ThumbnailUrl,
                ThemeColor = req.ThemeColor,
                Rows = req.Rows,
                Columns = req.Columns,
                Paylines = req.Paylines,
                WildType = req.WildType,
                IsActive = true
            };

            _context.SlotGames.Add(game);
            await _context.SaveChangesAsync(); // Salvăm ca să genereze un ID pentru joc

            // Salvăm fiecare simbol cu multiplicatorii lui
            foreach (var sym in req.Symbols)
            {
                _context.SlotSymbols.Add(new SlotSymbol
                {
                    Name = sym.ImageName.Replace(".png", "").Replace(".jpg", ""), // Un nume generic
                    ImageName = sym.ImageName,
                    Multiplier3 = sym.Multiplier3,
                    Multiplier4 = sym.Multiplier4,
                    Multiplier5 = sym.Multiplier5,
                    IsWild = sym.IsWild,
                    SlotGameId = game.Id
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateSlot(int id, [FromBody] AddSlotRequest req)
        {
            var game = await _context.SlotGames.Include(g => g.Symbols).FirstOrDefaultAsync(g => g.Id == id);
            if (game == null) return NotFound();

            // Actualizăm detaliile jocului
            game.Name = req.Name;
            game.ThumbnailUrl = req.ThumbnailUrl;
            game.ThemeColor = req.ThemeColor;
            game.Rows = req.Rows;
            game.Columns = req.Columns;
            game.Paylines = req.Paylines;
            game.WildType = req.WildType;

            // Aici era eroarea! Acum folosim req.Symbols în loc de req.SymbolImages
            if (req.Symbols != null && req.Symbols.Count > 0)
            {
                // Ștergem simbolurile vechi
                _context.SlotSymbols.RemoveRange(game.Symbols);

                // Adăugăm simbolurile noi cu tot cu noii multiplicatori
                foreach (var sym in req.Symbols)
                {
                    _context.SlotSymbols.Add(new SlotSymbol
                    {
                        Name = sym.ImageName.Replace(".png", "").Replace(".jpg", ""), // Nume generat din fișier
                        ImageName = sym.ImageName,
                        Multiplier3 = sym.Multiplier3,
                        Multiplier4 = sym.Multiplier4,
                        Multiplier5 = sym.Multiplier5,
                        IsWild = sym.IsWild,
                        SlotGameId = id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var game = await _context.SlotGames.FindAsync(id);
            if (game == null) return NotFound();
            _context.SlotGames.Remove(game);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("spin")]
        public async Task<IActionResult> Spin([FromBody] SlotSpinRequest req)
        {
            var user = await _context.Users.FindAsync(req.UserId);
            var game = await _context.SlotGames.Include(g => g.Symbols).FirstOrDefaultAsync(g => g.Id == req.GameId);

            if (user == null || game == null || game.Symbols.Count == 0)
                return BadRequest("Date invalide pentru Spin.");

            if (user.Balance < req.BetAmount)
                return BadRequest("Fonduri insuficiente.");

            // 1. Preluăm miza (aceasta se scade garantat)
            user.Balance -= req.BetAmount;
            decimal totalWin = 0;

            // 2. Generăm grila (folosind doar ImageName pentru UI)
            // Vom stoca temporar obiectele SlotSymbol în spate pentru a face logica ușor
            SlotSymbol[][] logicGrid = new SlotSymbol[game.Rows][];
            string[][] uiGrid = new string[game.Rows][];

            for (int r = 0; r < game.Rows; r++)
            {
                logicGrid[r] = new SlotSymbol[game.Columns];
                uiGrid[r] = new string[game.Columns];

                for (int c = 0; c < game.Columns; c++)
                {
                    // Alegem un simbol aleatoriu din lista jocului
                    var randomSymbol = game.Symbols.ElementAt(_random.Next(game.Symbols.Count));
                    logicGrid[r][c] = randomSymbol;
                    uiGrid[r][c] = randomSymbol.ImageName;
                }
            }

            if (game.WildType == "Expand_Column")
            {
                for (int c = 0; c < game.Columns; c++)
                {
                    // Verificăm dacă pe coloana curentă a picat vreun Wild
                    bool hasWild = false;
                    SlotSymbol wildSymbol = null;

                    for (int r = 0; r < game.Rows; r++)
                    {
                        if (logicGrid[r][c].IsWild)
                        {
                            hasWild = true;
                            wildSymbol = logicGrid[r][c];
                            break;
                        }
                    }

                    // Dacă are Wild, transformăm toată coloana în Wild (și pe logică, și pe interfață)
                    if (hasWild && wildSymbol != null)
                    {
                        for (int r = 0; r < game.Rows; r++)
                        {
                            logicGrid[r][c] = wildSymbol;
                            uiGrid[r][c] = wildSymbol.ImageName; // Coroana va apărea pe tot rândul
                        }
                    }
                }
            }
            else if (game.WildType == "Spread_3x3")
            {
                // BONUS: Mecanică pentru alte jocuri (Wild-ul infectează vecinii)
                var toTransform = new List<(int, int)>();
                SlotSymbol wildSymbol = null;

                for (int r = 0; r < game.Rows; r++)
                {
                    for (int c = 0; c < game.Columns; c++)
                    {
                        if (logicGrid[r][c].IsWild)
                        {
                            wildSymbol = logicGrid[r][c];
                            for (int dr = -1; dr <= 1; dr++)
                                for (int dc = -1; dc <= 1; dc++)
                                    if (r + dr >= 0 && r + dr < game.Rows && c + dc >= 0 && c + dc < game.Columns)
                                        toTransform.Add((r + dr, c + dc));
                        }
                    }
                }

                foreach (var (r, c) in toTransform)
                {
                    logicGrid[r][c] = wildSymbol;
                    uiGrid[r][c] = wildSymbol.ImageName;
                }
            }

            // 3. LOGICA DE CÂȘTIG (Citire pe rânduri, stânga-dreapta)
            // Căutăm dacă pe un rând avem 3, 4 sau 5 simboluri la fel, pornind de la coloana 0
            for (int r = 0; r < game.Rows; r++)
            {
                int matchCount = 1;
                SlotSymbol firstSymbol = logicGrid[r][0];

                // Dacă primul simbol e Wild, trebuie să căutăm următorul simbol non-Wild pentru a ști ce se potrivește
                SlotSymbol targetSymbol = firstSymbol;

                if (firstSymbol.IsWild)
                {
                    for (int c = 1; c < game.Columns; c++)
                    {
                        if (!logicGrid[r][c].IsWild)
                        {
                            targetSymbol = logicGrid[r][c];
                            break;
                        }
                    }
                }

                // Parcurgem restul coloanelor de pe acest rând
                for (int c = 1; c < game.Columns; c++)
                {
                    var currentSymbol = logicGrid[r][c];

                    // Dacă simbolul curent este la fel ca cel target SAU este Wild
                    if (currentSymbol.Id == targetSymbol.Id || currentSymbol.IsWild)
                    {
                        matchCount++;
                    }
                    else
                    {
                        break; // S-a rupt linia
                    }
                }

                // 4. Calculăm câștigul pentru linia curentă (dacă are minim 3)
                if (matchCount >= 3)
                {
                    decimal lineWin = 0;
                    if (matchCount == 3) lineWin = req.BetAmount * targetSymbol.Multiplier3;
                    else if (matchCount == 4) lineWin = req.BetAmount * targetSymbol.Multiplier4;
                    else if (matchCount >= 5) lineWin = req.BetAmount * targetSymbol.Multiplier5;

                    totalWin += lineWin;
                }
            }

            // 5. Adăugăm câștigul total
            if (totalWin > 0)
            {
                user.Balance += totalWin;
                _context.Transactions.Add(new Transaction
                {
                    UserId = user.Id,
                    Amount = totalWin,
                    Type = TransactionType.GameWin,
                    Timestamp = DateTime.Now
                });
            }

            // Înregistrăm miza pierdută
            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                Amount = req.BetAmount,
                Type = TransactionType.GameBet,
                Timestamp = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Ok(new SlotSpinResponse
            {
                Grid = uiGrid,
                NewBalance = user.Balance,
                TotalWin = totalWin,
                IsWin = totalWin > 0,
                Message = totalWin > 0 ? $"Ai câștigat {totalWin} RON!" : "Ghinion!"
            });
        }
        [HttpPost("gamble")]
        public async Task<IActionResult> Gamble([FromBody] GambleRequest req)
        {
            var user = await _context.Users.FindAsync(req.UserId);
            if (user == null || user.Balance < req.Amount)
                return BadRequest("Fonduri insuficiente pentru dublaj.");

            // Alegem o carte la întâmplare (0 = Roșu, 1 = Negru)
            bool isRed = _random.Next(2) == 0;
            string drawnColor = isRed ? "Red" : "Black";

            // Verificăm dacă a ghicit
            bool isWin = req.Guess == drawnColor;

            if (isWin)
            {
                // Banii i-au intrat deja la Spin, deci acum doar îi mai dăm o dată suma (dublăm)
                user.Balance += req.Amount;
            }
            else
            {
                // A pierdut, îi tragem câștigul din cont
                user.Balance -= req.Amount;
            }

            await _context.SaveChangesAsync();

            return Ok(new GambleResponse
            {
                IsWin = isWin,
                CardColor = drawnColor,
                NewBalance = user.Balance,
                WinAmount = isWin ? req.Amount * 2 : 0
            });
        }
    }

}