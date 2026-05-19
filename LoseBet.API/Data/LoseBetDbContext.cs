using Microsoft.EntityFrameworkCore;
using LoseBet.Core.Models;

namespace LoseBet.API.Data
{
    public class LoseBetDbContext : DbContext
    {
        public LoseBetDbContext(DbContextOptions<LoseBetDbContext> options) : base(options)
        {
        }

        // Astea vor deveni tabelele voastre reale din SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<SlotGame> SlotGames { get; set; }
        public DbSet<SlotSymbol> SlotSymbols { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<MinesSession> MinesSessions { get; set; }
        public DbSet<BlackjackSession> BlackjackSessions { get; set; }
        public DbSet<AviatorSessions> AviatorSessions { get; set; }
    }
}