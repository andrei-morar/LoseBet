using LoseBet.Core.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class AlbaNeagraWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        private bool _isPlaying = false;
        private readonly double[] _positions = { 0, 140, 280 }; // Coordonatele exacte pe Canvas (Stânga, Centru, Dreapta)

        public AlbaNeagraWindow()
        {
            InitializeComponent();
            RefreshBalance();
        }

        private async void RefreshBalance()
        {
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"{balance:F2} RON";
            GameService.NotifyBalanceChanged();
        }

        // ==========================================
        // ETAPA 1: START ȘI AMESTECARE
        // ==========================================
        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying) return;

            if (!decimal.TryParse(TxtBet.Text, out decimal betAmount) || betAmount <= 0)
            {
                MessageBox.Show("Setează o miză validă!");
                return;
            }

            _isPlaying = true;
            BtnStart.IsEnabled = false;
            TxtBet.IsEnabled = false;

            ResetBoard();

            // 1. Alegem un pahar la întâmplare să arătăm mingea la început
            TxtStatus.Text = "Atenție la bilă...";
            TxtStatus.Foreground = Brushes.Yellow;

            Random rnd = new Random();
            double startingPos = _positions[rnd.Next(0, 3)];
            StackPanel startingCup = GetCupAtPosition(startingPos);

            ShowBallInCup(startingCup);

            // Ridicăm cupa ca să vadă jucătorul
            startingCup.Margin = new Thickness(0, -40, 0, 40);
            await Task.Delay(1500);

            // Lăsăm cupa jos și ascundem mingea
            startingCup.Margin = new Thickness(0);
            HideAllBalls();
            await Task.Delay(300);

            // 2. Animația de amestecare
            TxtStatus.Text = "Amestecăm...";
            await ShuffleCupsAsync();

            // 3. Permitem jucătorului să aleagă
            TxtStatus.Text = "Unde e bila? Alege un pahar!";
            TxtStatus.Foreground = Brushes.White;
            BtnCup1.IsEnabled = true;
            BtnCup2.IsEnabled = true;
            BtnCup3.IsEnabled = true;
        }

        private async Task ShuffleCupsAsync()
        {
            Random r = new Random();
            StackPanel[] cups = { Cup1, Cup2, Cup3 };

            int shuffleCount = 8; // De câte ori schimbă paharele între ele
            for (int i = 0; i < shuffleCount; i++)
            {
                // Alegem două pahare la întâmplare să facă schimb
                int idx1 = r.Next(0, 3);
                int idx2 = r.Next(0, 3);
                while (idx1 == idx2) idx2 = r.Next(0, 3);

                await AnimateSwap(cups[idx1], cups[idx2]);
                await Task.Delay(40); // Scurtă pauză între mișcări
            }
        }

        // Animație matematică pentru a glisa două pahare pe Canvas
        private async Task AnimateSwap(StackPanel cup1, StackPanel cup2)
        {
            double start1 = Canvas.GetLeft(cup1);
            double start2 = Canvas.GetLeft(cup2);
            int steps = 10;
            int delayMs = 15; // Viteza animației

            for (int i = 1; i <= steps; i++)
            {
                double new1 = start1 + (start2 - start1) * ((double)i / steps);
                double new2 = start2 + (start1 - start2) * ((double)i / steps);

                Canvas.SetLeft(cup1, new1);
                Canvas.SetLeft(cup2, new2);

                await Task.Delay(delayMs);
            }

            // Ne asigurăm că la final se aliniază perfect la coordonate
            Canvas.SetLeft(cup1, start2);
            Canvas.SetLeft(cup2, start1);
        }

        // ==========================================
        // ETAPA 2: ALEGEREA JUCĂTORULUI
        // ==========================================
        private async void BtnCup_Click(object sender, RoutedEventArgs e)
        {
            // Blocăm butoanele să nu dea dublu click
            BtnCup1.IsEnabled = false;
            BtnCup2.IsEnabled = false;
            BtnCup3.IsEnabled = false;

            var button = sender as Button;
            var parentPanel = button.Parent as StackPanel;

            // Aflăm unde se află fizic pe ecran paharul ales (Poziția 1, 2 sau 3)
            double currentX = Canvas.GetLeft(parentPanel);
            int chosenLogicalPosition = currentX == _positions[0] ? 1 : (currentX == _positions[1] ? 2 : 3);

            decimal betAmount = decimal.Parse(TxtBet.Text);

            try
            {
                // Trimitem poziția la server. Serverul va da cu zarul și va zice dacă mingea e acolo.
                var payload = new
                {
                    UserId = UserSession.UserId,
                    BetAmount = betAmount,
                    ChosenCup = chosenLogicalPosition
                };

                var response = await _httpClient.PostAsJsonAsync("api/fastgames/albaneagra", payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AlbaNeagraResponseDTO>();

                    // Găsim paharul care stă pe poziția câștigătoare (returnată de API) și arătăm mingea acolo
                    StackPanel winningCupPanel = GetCupAtPosition(_positions[result.WinningCup - 1]);
                    ShowBallInCup(winningCupPanel);

                    // Ridicăm paharele ca să se vadă clar
                    parentPanel.Margin = new Thickness(0, -40, 0, 40);
                    if (parentPanel != winningCupPanel)
                    {
                        winningCupPanel.Margin = new Thickness(0, -40, 0, 40);
                    }

                    if (result.IsWin)
                    {
                        TxtStatus.Text = $"FELICITĂRI! Ai câștigat {betAmount * 3:F2} RON!";
                        TxtStatus.Foreground = Brushes.Lime;
                    }
                    else
                    {
                        TxtStatus.Text = "AI PIERDUT! Mingea era în altă parte.";
                        TxtStatus.Foreground = Brushes.Red;
                    }

                    RefreshBalance();
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync(), "Eroare");
                    TxtStatus.Text = "Eroare la procesare.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare de conexiune: " + ex.Message);
                TxtStatus.Text = "Eroare rețea.";
            }

            // Așteptăm să vadă rezultatul, apoi resetăm starea
            await Task.Delay(3000);
            ResetBoard();
            TxtStatus.Text = "Alege miza și apasă START!";
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e94560"));

            BtnStart.IsEnabled = true;
            TxtBet.IsEnabled = true;
            _isPlaying = false;
        }

        // ==========================================
        // UTILAJE MINORE
        // ==========================================
        private StackPanel GetCupAtPosition(double x)
        {
            if (Canvas.GetLeft(Cup1) == x) return Cup1;
            if (Canvas.GetLeft(Cup2) == x) return Cup2;
            return Cup3;
        }

        private void ShowBallInCup(StackPanel cup)
        {
            if (cup == Cup1) Ball1.Visibility = Visibility.Visible;
            if (cup == Cup2) Ball2.Visibility = Visibility.Visible;
            if (cup == Cup3) Ball3.Visibility = Visibility.Visible;
        }

        private void HideAllBalls()
        {
            Ball1.Visibility = Visibility.Hidden;
            Ball2.Visibility = Visibility.Hidden;
            Ball3.Visibility = Visibility.Hidden;
        }

        private void ResetBoard()
        {
            HideAllBalls();
            Cup1.Margin = new Thickness(0);
            Cup2.Margin = new Thickness(0);
            Cup3.Margin = new Thickness(0);

            // Le așezăm înapoi la coordonatele de fabrică
            Canvas.SetLeft(Cup1, _positions[0]);
            Canvas.SetLeft(Cup2, _positions[1]);
            Canvas.SetLeft(Cup3, _positions[2]);
        }

        private void BtnLowerBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtBet.Text, out decimal val) && val > 1) TxtBet.Text = (val - 1).ToString("F2");
        }

        private void BtnHigherBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtBet.Text, out decimal val)) TxtBet.Text = (val + 1).ToString("F2");
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow("", "");
            dashboard.Show();
            this.Close();
        }
    }

    public class AlbaNeagraResponseDTO
    {
        public int WinningCup { get; set; }
        public bool IsWin { get; set; }
        public decimal NewBalance { get; set; }
        public string Message { get; set; }
    }
}