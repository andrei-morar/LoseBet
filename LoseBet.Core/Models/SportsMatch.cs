namespace LoseBet.Core.Models
{
    public class SportsMatch
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;

        // Cotele principale
        public decimal Odds1 { get; set; } // Victorie Gazde
        public decimal OddsX { get; set; } // Egal
        public decimal Odds2 { get; set; } // Victorie Oaspeți

        public bool IsFinished { get; set; } = false;
        public int HomeScore { get; set; } = 0;
        public int AwayScore { get; set; } = 0;
    }
}