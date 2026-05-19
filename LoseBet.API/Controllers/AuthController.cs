using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;
using System;
using System.Threading.Tasks;

namespace LoseBet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LoseBetDbContext _context;

        public AuthController(LoseBetDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. Validăm CNP-ul 
            if (!IdentityValidator.ValidateCNP(request.Cnp, request.DataNasterii))
                return BadRequest("CNP invalid, sub 18 ani, sau data nașterii nu corespunde cu CNP-ul.");

            // 2. Verificăm dacă email-ul sau username-ul există deja
            if (await _context.Users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username))
                return BadRequest("Acest email sau username este deja folosit.");

            // 3. Creăm utilizatorul
            var newUser = new User
            {
                FirstName = request.Prenume,
                LastName = request.Nume,
                BirthDate = DateTime.SpecifyKind(request.DataNasterii, DateTimeKind.Utc),
                Email = request.Email,
                Username = request.Username,
                PasswordHash = request.Password,
                CNP = request.Cnp,
                Balance = 0, // Bonus de bun venit
                Currency = "RON",
                IsAgeVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Role = "Player"
            };

            // 4. Salvăm în baza de date
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Contul a fost creat cu succes!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized("Username sau parolă incorecte!");

            return Ok(new
            {
                Id = user.Id, // <--- ADAUGĂ LINIA ASTA!!!
                Message = $"Login reușit! Bine ai venit, {user.Username}",
                Username = user.Username,
                Balance = user.Balance,
                Role = user.Role
            });
        }

        [HttpPost("update-balance")]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null) return NotFound("User negăsit");

            user.Balance = request.NewBalance;

            await _context.SaveChangesAsync(); // ASTA e linia care trimite banii în cloud la Supabase!
            return Ok(new { Balance = user.Balance });
        }

        
    }

    // Definim DTO-urile noastre aici în fișier (sau asigură-te că nu importi alt RegisterRequest)
    public class RegisterRequest
    {
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string Cnp { get; set; } = string.Empty;
        public DateTime DataNasterii { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateBalanceRequest
    {
        public string Username { get; set; }
        public decimal NewBalance { get; set; }
    }
}
