using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using System.Threading.Tasks;
using System.Linq;

namespace LoseBet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly LoseBetDbContext _context;

        public AdminController(LoseBetDbContext context)
        {
            _context = context;
        }

        // Metodă ajutătoare pentru a verifica dacă ești Admin
        private async Task<bool> IsUserAdmin(string adminUsername)
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == adminUsername);
            return admin != null && admin.Role == "Admin";
        }

        // 1. GET: Luăm toți jucătorii
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string adminUsername)
        {
            if (!await IsUserAdmin(adminUsername))
                return Unauthorized("Nu ai drepturi de Administrator!");

            var users = await _context.Users
                .Select(u => new {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Balance,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. POST: Banează / Debanează un user
        [HttpPost("toggle-ban")]
        public async Task<IActionResult> ToggleBan([FromBody] AdminActionRequest request)
        {
            if (!await IsUserAdmin(request.AdminUsername))
                return Unauthorized("Nu ai drepturi de Administrator!");

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.TargetUsername);
            if (targetUser == null) return NotFound("Userul nu există.");

            targetUser.IsActive = !targetUser.IsActive;
            await _context.SaveChangesAsync();

            string statusText = targetUser.IsActive ? "Debanat" : "Banat";
            return Ok(new { Message = $"Utilizatorul {targetUser.Username} a fost {statusText}." });
        }

        // 3. POST: Setează balanța exactă a unui user
        [HttpPost("update-balance")]
        public async Task<IActionResult> ForceUpdateBalance([FromBody] AdminMoneyRequest request)
        {
            if (!await IsUserAdmin(request.AdminUsername))
                return Unauthorized("Nu ai drepturi de Administrator!");

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.TargetUsername);
            if (targetUser == null) return NotFound("Userul nu există.");

            // AICI E MAGIA: Setăm suma direct, nu o mai adunăm!
            targetUser.Balance = request.NewBalance;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Balanța lui {targetUser.Username} a fost setată exact la {targetUser.Balance} RON." });
        }

        // 4. POST: Schimbă rolul unui user (Admin/Player)
        [HttpPost("update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] AdminRoleRequest request)
        {
            if (!await IsUserAdmin(request.AdminUsername))
                return Unauthorized("Nu ai drepturi de Administrator!");

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.TargetUsername);
            if (targetUser == null) return NotFound("Userul nu există.");

            // Protecție: să nu-ți scoți singur gradul de admin
            if (request.AdminUsername == request.TargetUsername && request.NewRole != "Admin")
                return BadRequest("Nu îți poți scoate singur rolul de Admin!");

            targetUser.Role = request.NewRole;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Rolul lui {targetUser.Username} a fost modificat în {request.NewRole}." });
        }
    }

    // --- DTO-uri pentru cererile de Admin ---
    public class AdminActionRequest
    {
        public string AdminUsername { get; set; } = string.Empty;
        public string TargetUsername { get; set; } = string.Empty;
    }

    public class AdminMoneyRequest : AdminActionRequest
    {
        // Am schimbat numele ca să aibă sens
        public decimal NewBalance { get; set; }
    }

    // DTO-ul NOU pentru schimbarea rolului
    public class AdminRoleRequest : AdminActionRequest
    {
        public string NewRole { get; set; } = string.Empty;
    }
}