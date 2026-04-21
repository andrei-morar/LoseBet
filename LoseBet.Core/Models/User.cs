namespace LoseBet.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        // Date de Logare
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Date Personale (pentru realism)
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }

        // Portofel
        public decimal Balance { get; set; } = 0.00m;
        public string Currency { get; set; } = "RON"; // Realist: RON, EUR, USD

        // Status Cont
        public bool IsActive { get; set; } = true;
        public bool IsSelfExcluded { get; set; } = false; // Funcție reală de cazino

        public string CNP { get; set; } = string.Empty;
        public bool IsAgeVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}