namespace LoseBet.Core.Models
{
    public class BetSelection
    {
        public int Id { get; set; }
        public int BetTicketId { get; set; }
        public int SportsMatchId { get; set; }

        // Ce a pariat: "1", "X", sau "2"
        public string Pick { get; set; } = string.Empty;

        // Salvăm cota exactă din momentul parierii (în caz că adminul o schimbă mai târziu)
        public decimal Odds { get; set; }

        // Status selecție: "Pending", "Won", "Lost"
        public string Status { get; set; } = "Pending";

        // Relații pentru Entity Framework
        public virtual SportsMatch Match { get; set; }
    }
}