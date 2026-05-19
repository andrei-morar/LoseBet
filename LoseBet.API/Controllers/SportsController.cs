using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;

namespace LoseBet.API.Controllers
{
    // --- DTO-uri pentru cererile de rețea ---
    public class AddMatchRequest
    {
        public string HomeTeam { get; set; } = "";
        public string AwayTeam { get; set; } = "";
        public decimal Odds1 { get; set; }
        public decimal OddsX { get; set; }
        public decimal Odds2 { get; set; }
    }

    public class PlaceBetRequest
    {
        public int UserId { get; set; }
        public decimal Stake { get; set; }
        public List<SelectionDTO> Selections { get; set; } = new();
    }

    public class SelectionDTO
    {
        public int MatchId { get; set; }
        public string Pick { get; set; } = ""; // "1", "X" sau "2"
    }

    // --- CONTROLLERUL ---
    [ApiController]
    [Route("api/[controller]")]
    public class SportsController : ControllerBase
    {
        private readonly LoseBetDbContext _context;
        private readonly Random _random = new Random();

        public SportsController(LoseBetDbContext context)
        {
            _context = context;
        }

        // 1. Aducem meciurile disponibile (pentru jucători)
        [HttpGet("active-matches")]
        public async Task<IActionResult> GetActiveMatches()
        {
            var matches = await _context.SportsMatches.Where(m => !m.IsFinished).ToListAsync();
            return Ok(matches);
        }

        // 2. Aducem biletele unui jucător (Istoric)
        [HttpGet("tickets/{userId}")]
        public async Task<IActionResult> GetUserTickets(int userId)
        {
            var tickets = await _context.BetTickets
                .Include(t => t.Selections)
                .ThenInclude(s => s.Match)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(tickets);
        }

        // 3. ADMIN: Adaugă meci nou
        [HttpPost("admin/add-match")]
        public async Task<IActionResult> AddMatch([FromBody] AddMatchRequest req)
        {
            var match = new SportsMatch
            {
                HomeTeam = req.HomeTeam,
                AwayTeam = req.AwayTeam,
                Odds1 = req.Odds1,
                OddsX = req.OddsX,
                Odds2 = req.Odds2
            };
            _context.SportsMatches.Add(match);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // 4. JUCĂTOR: Plasează bilet
        [HttpPost("place-bet")]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetRequest req)
        {
            var user = await _context.Users.FindAsync(req.UserId);
            if (user == null || user.Balance < req.Stake || req.Selections.Count == 0)
                return BadRequest("Date invalide sau fonduri insuficiente.");

            user.Balance -= req.Stake; // Luăm banii

            var ticket = new BetTicket
            {
                UserId = req.UserId,
                Stake = req.Stake,
                TotalOdds = 1,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            foreach (var sel in req.Selections)
            {
                var match = await _context.SportsMatches.FindAsync(sel.MatchId);
                if (match == null || match.IsFinished) return BadRequest("Un meci nu mai este disponibil.");

                decimal selectionOdds = sel.Pick == "1" ? match.Odds1 : (sel.Pick == "X" ? match.OddsX : match.Odds2);
                ticket.TotalOdds *= selectionOdds; // Înmulțim cotele

                ticket.Selections.Add(new BetSelection
                {
                    SportsMatchId = match.Id,
                    Pick = sel.Pick,
                    Odds = selectionOdds,
                    Status = "Pending"
                });
            }

            ticket.PotentialWin = ticket.Stake * ticket.TotalOdds;

            _context.BetTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Bilet plasat cu succes!" });
        }

        // 5. ADMIN: SIMULAREA MECIULUI ȘI PLATA BILETELOR
        [HttpPost("admin/simulate/{matchId}")]
        public async Task<IActionResult> SimulateMatch(int matchId)
        {
            var match = await _context.SportsMatches.FindAsync(matchId);
            if (match == null || match.IsFinished) return BadRequest("Meci invalid.");

            // Generăm scor random (0-4 goluri pentru fiecare)
            match.HomeScore = _random.Next(0, 5);
            match.AwayScore = _random.Next(0, 5);
            match.IsFinished = true;

            // Stabilim semnul câștigător: 1, X sau 2
            string winningPick = match.HomeScore > match.AwayScore ? "1" : (match.HomeScore == match.AwayScore ? "X" : "2");

            // Căutăm toate selecțiile făcute pe acest meci
            var selections = await _context.BetSelections
                .Include(s => s.Match)
                .Where(s => s.SportsMatchId == matchId && s.Status == "Pending")
                .ToListAsync();

            foreach (var sel in selections)
            {
                sel.Status = sel.Pick == winningPick ? "Won" : "Lost";
            }
            await _context.SaveChangesAsync(); // Salvăm selecțiile

            // ACUM VALIDĂM BILETELE (Dacă a pierdut un meci, cade tot biletul)
            var ticketsToUpdate = await _context.BetTickets
                .Include(t => t.Selections)
                .Where(t => t.Status == "Pending")
                .ToListAsync();

            foreach (var ticket in ticketsToUpdate)
            {
                if (ticket.Selections.Any(s => s.Status == "Lost"))
                {
                    ticket.Status = "Lost"; // Ne pare rău, bilet aruncat la gunoi
                }
                else if (ticket.Selections.All(s => s.Status == "Won"))
                {
                    ticket.Status = "Won"; // BILET VERDE!
                    var ticketOwner = await _context.Users.FindAsync(ticket.UserId);
                    if (ticketOwner != null)
                    {
                        ticketOwner.Balance += ticket.PotentialWin; // Îi dăm banii

                        _context.Transactions.Add(new Transaction
                        {
                            UserId = ticketOwner.Id,
                            Amount = ticket.PotentialWin,
                            Type = TransactionType.GameWin,
                            Timestamp = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Meci simulat: {match.HomeTeam} {match.HomeScore} - {match.AwayScore} {match.AwayTeam}" });
        }
    }
}