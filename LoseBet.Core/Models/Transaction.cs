namespace LoseBet.Core.Models
{
    public enum TransactionType { Deposit, Withdrawal, GameBet, GameWin }

    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string PaymentMethod { get; set; } = "Card"; // ex: Visa, Skrill, Crypto

        public string TransactionReference { get; set; } = Guid.NewGuid().ToString(); // Cod unic de tranzacție
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}