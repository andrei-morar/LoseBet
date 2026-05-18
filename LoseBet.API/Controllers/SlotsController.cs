using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;

namespace LoseBet.API.Controllers
{
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
        public List<string> SymbolImages { get; set; } = new();
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
            var game = new SlotGame { Name = req.Name, ThumbnailUrl = req.ThumbnailUrl, ThemeColor = req.ThemeColor, Rows = req.Rows, Columns = req.Columns, Paylines = req.Paylines, WildType = req.WildType, IsActive = true };
            _context.SlotGames.Add(game);
            await _context.SaveChangesAsync();

            foreach (var img in req.SymbolImages)
                _context.SlotSymbols.Add(new SlotSymbol { ImageName = img, SlotGameId = game.Id });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateSlot(int id, [FromBody] AddSlotRequest req)
        {
            var game = await _context.SlotGames.Include(g => g.Symbols).FirstOrDefaultAsync(g => g.Id == id);
            if (game == null) return NotFound();

            game.Name = req.Name; game.ThumbnailUrl = req.ThumbnailUrl; game.Rows = req.Rows; game.Columns = req.Columns;

            if (req.SymbolImages.Count > 0)
            {
                _context.SlotSymbols.RemoveRange(game.Symbols);
                foreach (var img in req.SymbolImages) _context.SlotSymbols.Add(new SlotSymbol { ImageName = img, SlotGameId = id });
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            var game = await _context.SlotGames.Include(g => g.Symbols).FirstOrDefaultAsync(g => g.Id == req.GameId);
            if (user == null || game == null || game.Symbols.Count == 0) return BadRequest();

            user.Balance -= req.BetAmount;
            string[] imgs = game.Symbols.Select(s => s.ImageName).ToArray();
            string[][] grid = new string[game.Rows][];
            for (int r = 0; r < game.Rows; r++)
            {
                grid[r] = new string[game.Columns];
                for (int c = 0; c < game.Columns; c++) grid[r][c] = imgs[_random.Next(imgs.Length)];
            }

            await _context.SaveChangesAsync();
            return Ok(new SlotSpinResponse { Grid = grid, NewBalance = user.Balance, TotalWin = 0, IsWin = false });
        }
    }
}