using System;
using System.Collections.Generic; // IMPORTANT

namespace LoseBet.Core.Models
{
    public class SlotGame
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string ThemeColor { get; set; } = "#1E1E1E";
        public int Rows { get; set; } = 3;
        public int Columns { get; set; } = 5;
        public int Paylines { get; set; } = 20;
        public string WildType { get; set; } = "Classic_1x1";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // NOU: Lista de simboluri a acestui joc
        public virtual ICollection<SlotSymbol> Symbols { get; set; } = new List<SlotSymbol>();
    }
}