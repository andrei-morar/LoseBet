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

        // Aici "injectăm" baza de date ca să o putem folosi
        public AuthController(LoseBetDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. Validăm CNP-ul și vârsta (18+)
            /* NOTĂ: Asigură-te că metoda ValidateCNP din IdentityValidator este publică și statică! */
            if (!IdentityValidator.ValidateCNP(request.CNP))
                 return BadRequest("CNP invalid sau vârsta este sub 18 ani.");

            // 2. Verificăm dacă email-ul există deja în baza de date
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Acest email este deja folosit.");

            // 3. Creăm utilizatorul (Atenție: în producție parola trebuie criptată, aici o lăsăm simplă pentru test)
            var newUser = new User
            {
                Email = request.Email,
                Username = request.Email.Split('@')[0], // Luăm prima parte din email ca username
                PasswordHash = request.Password,
                CNP = request.CNP,
                Balance = 0, // Primește 0 lei la început
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
            // Căutăm userul în baza de date după email și parolă
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized("Email sau parolă incorecte!");

            return Ok($"Login reușit! Bine ai venit, {user.Username}. Sold: {user.Balance} RON");
        }
    }

    // Acestea sunt DTO-uri (Data Transfer Objects) - "Pachetele" pe care le primim de la Frontend
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CNP { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}