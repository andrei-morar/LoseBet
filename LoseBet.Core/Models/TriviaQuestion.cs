namespace LoseBet.Core.Models
{
    public class TriviaQuestion
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty; // ex: Istorie, Geografie
        public string Text { get; set; } = string.Empty;     // Întrebarea în sine
        public int Type { get; set; }                        // 1 = Aproximare (Ani/Cifre), 2 = 4 Variante

        // Pentru tipul 2 (Grilă). La tipul 1 vor rămâne goale (null)
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        public string CorrectAnswer { get; set; } = string.Empty; // Răspunsul corect (textul variantei sau numărul exact)
    }
}