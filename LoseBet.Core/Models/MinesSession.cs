using System.ComponentModel.DataAnnotations;

namespace LoseBet.Core.Models
{
    public class MinesSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public decimal BetAmount { get; set; }

        public int BombCount { get; set; }

        // Locația bombelor (ex: "1,4,12")
        public string BombLocations { get; set; } = string.Empty;

        // Căsuțele deschise (ex: "2,5")
        public string RevealedIndices { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsGameOver { get; set; } = false;

        public decimal CurrentMultiplier { get; set; } = 1.0m;
    }
}