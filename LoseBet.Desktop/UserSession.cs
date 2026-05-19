namespace LoseBet.Desktop
{
    public static class UserSession
    {
        public static string Role { get; set; } = "Player";
        // Adăugăm UserId pentru a-l trimite la API-ul tău
        public static int UserId { get; set; }
    }
}