using System.ComponentModel.DataAnnotations;

namespace LoseBet.Core.Models
{
    public class BlackjackSession
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal BetAmount { get; set; }

        // Stocăm cărțile ca string-uri separate prin virgulă (ex: "10,7,A")
        public string UserHand { get; set; } = string.Empty;
        public string DealerHand { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "In Progress"; // "Win", "Lose", "Draw", "Bust"
    }
}