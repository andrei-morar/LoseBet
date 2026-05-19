namespace LoseBet.Core.Models
{
    public class SlotSymbol
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // ex: "Cireașă", "Șeptar"
        public string ImageName { get; set; } = string.Empty; // ex: "cherry.png"
        public decimal Multiplier3 { get; set; } // Cât plătește la 3 simboluri
        public decimal Multiplier4 { get; set; } // Cât plătește la 4
        public decimal Multiplier5 { get; set; } // Cât plătește la 5
        public bool IsWild { get; set; } = false;

        // Legătura cu jocul
        public int SlotGameId { get; set; }
    }
}