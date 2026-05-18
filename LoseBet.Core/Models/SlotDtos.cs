namespace LoseBet.Core.Models
{
    public class SlotSpinRequest
    {
        public string Username { get; set; }
        public int GameId { get; set; }
        public decimal BetAmount { get; set; }
    }

    public class SlotSpinResponse
    {
        public string[][] Grid { get; set; }
        public decimal TotalWin { get; set; }
        public decimal NewBalance { get; set; }
        public bool IsWin { get; set; }
        public List<string> WinningLines { get; set; }
    }
}