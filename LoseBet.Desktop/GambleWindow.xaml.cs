using LoseBet.Core.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class GambleWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };
        private decimal _currentAmount;

        public GambleWindow(decimal amount)
        {
            InitializeComponent();
            _currentAmount = amount;
            TxtGambleAmount.Text = $"{_currentAmount:F2} RON";
        }

        private async void BtnRed_Click(object sender, RoutedEventArgs e) => await ProcessGamble("Red");
        private async void BtnBlack_Click(object sender, RoutedEventArgs e) => await ProcessGamble("Black");

        private async Task ProcessGamble(string guess)
        {
            BtnRed.IsEnabled = BtnBlack.IsEnabled = BtnCollect.IsEnabled = false;

            try
            {
                var payload = new { UserId = UserSession.UserId, Amount = _currentAmount, Guess = guess };
                var resp = await _httpClient.PostAsJsonAsync("api/slots/gamble", payload);

                if (resp.IsSuccessStatusCode)
                {
                    var res = await resp.Content.ReadFromJsonAsync<GambleResponseDTO>();

                    // Afișăm cartea
                    TxtCardValue.Text = res.CardColor == "Red" ? "♥" : "♠";
                    TxtCardValue.Foreground = res.CardColor == "Red" ? Brushes.Red : Brushes.Black;

                    if (res.IsWin)
                    {
                        _currentAmount = res.WinAmount;
                        TxtGambleAmount.Text = $"{_currentAmount:F2} RON";
                        TxtResult.Text = "AI GHICIT! Poți dubla din nou sau colecta.";
                        TxtResult.Foreground = Brushes.Lime;

                        BtnRed.IsEnabled = BtnBlack.IsEnabled = BtnCollect.IsEnabled = true;
                    }
                    else
                    {
                        TxtResult.Text = "AI PIERDUT!";
                        TxtResult.Foreground = Brushes.Red;

                        // Așteptăm 1.5 secunde să vadă cartea, apoi închidem
                        await Task.Delay(1500);
                        this.DialogResult = false;
                        this.Close();
                    }

                    GameService.NotifyBalanceChanged(); // Anunțăm că s-au mișcat banii
                }
            }
            catch { MessageBox.Show("Eroare de conexiune!"); BtnRed.IsEnabled = BtnBlack.IsEnabled = BtnCollect.IsEnabled = true; }
        }

        private void BtnCollect_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }

    public class GambleResponseDTO
    {
        public bool IsWin { get; set; }
        public string CardColor { get; set; }
        public decimal NewBalance { get; set; }
        public decimal WinAmount { get; set; }
    }
}// samrale
