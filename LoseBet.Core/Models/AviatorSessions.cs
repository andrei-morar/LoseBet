using System.ComponentModel.DataAnnotations;

namespace LoseBet.Core.Models
{
    public class AviatorSessions
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal BetAmount { get; set; }

        // Punctul la care avionul explodează (generat la start)
        public decimal CrashPoint { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime StartTime { get; set; } = DateTime.Now;
    }
}