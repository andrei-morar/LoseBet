using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoseBet.API.Data;
using LoseBet.Core.Models;

namespace LoseBet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriviaController : ControllerBase
    {
        private readonly LoseBetDbContext _context;

        public TriviaController(LoseBetDbContext context)
        {
            _context = context;
        }

        // 1. Aducem toate întrebările (Pentru joc)
        [HttpGet("questions")]
        public async Task<IActionResult> GetQuestions()
        {
            var questions = await _context.TriviaQuestions.ToListAsync();
            return Ok(questions);
        }

        // 2. ADMIN: Adaugă o întrebare nouă
        [HttpPost("admin/add")]
        public async Task<IActionResult> AddQuestion([FromBody] TriviaQuestion q)
        {
            if (string.IsNullOrWhiteSpace(q.Text) || string.IsNullOrWhiteSpace(q.CorrectAnswer))
                return BadRequest("Întrebarea și răspunsul corect sunt obligatorii!");

            _context.TriviaQuestions.Add(q);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Întrebare adăugată cu succes în baza de date!" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var q = await _context.TriviaQuestions.FindAsync(id);
            if (q == null) return NotFound();
            _context.TriviaQuestions.Remove(q);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}