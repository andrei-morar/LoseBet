using System;
using System.Collections.Generic;

namespace LoseBet.Core.Models
{
    public class BetTicket
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Stake { get; set; } // Cât a pariat
        public decimal TotalOdds { get; set; } // Cota totală a biletului
        public decimal PotentialWin { get; set; } // Cât poate câștiga

        // Status: "Pending" (În desfășurare), "Won" (Câștigat), "Lost" (Pierdut)
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Legătura cu selecțiile de pe bilet
        public virtual ICollection<BetSelection> Selections { get; set; } = new List<BetSelection>();
    }
}