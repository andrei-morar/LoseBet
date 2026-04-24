using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;

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
            // 1. Validăm CNP-ul (Atenție: am modificat în request.Cnp cu litere mici, cum e în DTO)
            // Trimitem și data pe care ai introdus-o tu pe Frontend
            if (!IdentityValidator.ValidateCNP(request.Cnp, request.DataNasterii))
                return BadRequest("CNP invalid, sub 18 ani, sau data nașterii nu corespunde cu CNP-ul.");

            // 2. Verificăm dacă email-ul sau username-ul există deja în baza de date
            if (await _context.Users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username))
                return BadRequest("Acest email sau username este deja folosit.");

            // 3. Creăm utilizatorul
            var newUser = new User
            {
                Email = request.Email,
                Username = request.Username, // Folosim Username-ul trimis de tine din WPF!
                PasswordHash = request.Password,
                CNP = request.Cnp, // Am pus request.Cnp (litere mici)
                Balance = 0,
                IsAgeVerified = true
            };

            // 4. Salvăm în baza de date
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Contul a fost creat cu succes!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Căutăm userul în baza de date după USERNAME (înainte era Email) și parola
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized("Username sau parolă incorecte!");

            return Ok($"Login reușit! Bine ai venit, {user.Username}. Sold: {user.Balance} RON");
        }
    }

    // --- DTO-urile actualizate la fix ---
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
}