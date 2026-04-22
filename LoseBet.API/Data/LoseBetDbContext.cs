using Microsoft.EntityFrameworkCore;
using LoseBet.Core.Models; // Importăm modelele pe care tocmai le-ai făcut

namespace LoseBet.API.Data
{
    public class LoseBetDbContext : DbContext
    {
        public LoseBetDbContext(DbContextOptions<LoseBetDbContext> options) : base(options)
        {
        }

        // Astea vor deveni tabelele voastre reale din SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<MinesSession> MinesSessions { get; set; }
        public DbSet<BlackjackSession> BlackjackSessions { get; set; }
        public DbSet<AviatorSessions> AviatorSessions { get; set; }
    }
}