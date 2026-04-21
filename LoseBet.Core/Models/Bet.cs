namespace LoseBet.Core.Models
{
    public enum BetStatus { Pending, Won, Lost, Cancelled }

    public class Bet
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string GameName { get; set; } = string.Empty; // ex: "Fruit Multiplier", "European Roulette"
        public decimal Amount { get; set; } // Cât a pariat
        public decimal Multiplier { get; set; } // ex: 2.0x, 35.0x
        public decimal Payout { get; set; } // Cât a primit înapoi (0 dacă e Lost)

        public BetStatus Status { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.Now;
    }
}