namespace LoseBet.Desktop
{
    // Aici stocăm datele care trebuie să fie accesibile din ORICE fereastră
    public static class UserSession
    {
        public static string Role { get; set; } = "Player"; // Default
    }
}