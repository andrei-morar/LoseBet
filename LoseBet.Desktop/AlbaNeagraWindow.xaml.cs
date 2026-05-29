using LoseBet.Core.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LoseBet.Desktop
{
    public partial class AlbaNeagraWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7000/")
        };

        private bool _isPlaying = false;

        // Pozitiile initiale (Canvas.Left) ale celor 3 pahare
        private readonly double[] _positions = { 0, 195, 390 };

        // Canvas.Top de baza al paharelor (cand sunt jos pe masa)
        private const double CupTopBase = 20;

        // Cat de mult se ridica un pahar cand e "lifted"
        private const double CupLiftAmount = 80;

        public AlbaNeagraWindow()
        {
            InitializeComponent();
            RefreshBalance();
        }

        private async void RefreshBalance()
        {
            try
            {
                decimal balance = await GameService.GetBalanceAsync();
                TxtBalance.Text = $"{balance:F2} RON";
                GameService.NotifyBalanceChanged();
            }
            catch
            {
                // Ignoram erorile silentios
            }
        }

        // ==========================================
        // DEALER ANIMATIE GURA
        // ==========================================
        private void SetDealerMouth(string expression)
        {
            // "neutral" | "smile" | "sad"
            switch (expression)
            {
                case "smile":
                    DealerMouth.Data = Geometry.Parse("M 258,26 Q 270,36 282,26");
                    break;
                case "sad":
                    DealerMouth.Data = Geometry.Parse("M 258,32 Q 270,24 282,32");
                    break;
                default:
                    DealerMouth.Data = Geometry.Parse("M 258,28 Q 270,33 282,28");
                    break;
            }
        }

        // ==========================================
        // RIDICAREA / COBORAREA UNUI PAHAR
        // ==========================================
        private async Task LiftCupAsync(StackPanel cup, bool lift, int durationMs = 200)
        {
            double startTop = Canvas.GetTop(cup);
            double endTop = lift ? (CupTopBase - CupLiftAmount) : CupTopBase;
            int steps = 12;
            int stepDelay = durationMs / steps;

            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                // Easing: ease-out
                double eased = 1 - Math.Pow(1 - t, 2);
                Canvas.SetTop(cup, startTop + (endTop - startTop) * eased);
                await Task.Delay(stepDelay);
            }
            Canvas.SetTop(cup, endTop);
        }

        // ==========================================
        // ETAPA 1: START SI AMESTECARE
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
            SetDealerMouth("neutral");

            // 1. Alegem un pahar la intamplare sa aratam bila
            TxtStatus.Text = "ATENȚIE LA BILĂ...";
            TxtStatus.Foreground = Brushes.White;

            Random rnd = new Random();
            double startingPos = _positions[rnd.Next(0, 3)];
            StackPanel cup = GetCupAtPosition(startingPos);

            if (cup != null)
            {
                ShowBallInCup(cup);

                // Ridicam cupa pentru a arata bila
                await LiftCupAsync(cup, true, 250);
                await Task.Delay(1400);

                // Coboram cupa si ascundem bila
                await LiftCupAsync(cup, false, 200);
            }
            HideAllBalls();
            await Task.Delay(300);

            // 2. Animatia de amestecare
            TxtStatus.Text = "AMESTECĂM...";
            TxtStatus.Foreground = Brushes.Yellow;
            await ShuffleCupsAsync();

            // 3. Permitem jucatorului sa aleaga
            TxtStatus.Text = "UNDE E BILA? ALEGE UN PAHAR!";
            TxtStatus.Foreground = Brushes.White;
            BtnCup1.IsEnabled = true;
            BtnCup2.IsEnabled = true;
            BtnCup3.IsEnabled = true;
        }

        private async Task ShuffleCupsAsync()
        {
            Random r = new Random();
            StackPanel[] cups = { Cup1, Cup2, Cup3 };

            int shuffleCount = 9;
            for (int i = 0; i < shuffleCount; i++)
            {
                int idx1 = r.Next(0, 3);
                int idx2 = r.Next(0, 3);
                while (idx1 == idx2) idx2 = r.Next(0, 3);

                await AnimateSwapWithArc(cups[idx1], cups[idx2]);
                await Task.Delay(35);
            }
        }

        // Swap cu arc: paharele se ridica usor in mijlocul miscarii
        private async Task AnimateSwapWithArc(StackPanel cup1, StackPanel cup2)
        {
            double startLeft1 = Canvas.GetLeft(cup1);
            double startLeft2 = Canvas.GetLeft(cup2);
            double baseTop = CupTopBase;
            double arcHeight = 28; // cat de sus se ridica in arc

            int steps = 14;
            int delayMs = 13;

            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;

                // Interpolare liniara pe X
                double newLeft1 = startLeft1 + (startLeft2 - startLeft1) * t;
                double newLeft2 = startLeft2 + (startLeft1 - startLeft2) * t;

                // Arc parabolic pe Y: ridicare maxima la t=0.5
                double arc = arcHeight * Math.Sin(Math.PI * t);
                double newTop = baseTop - arc;

                Canvas.SetLeft(cup1, newLeft1);
                Canvas.SetLeft(cup2, newLeft2);
                Canvas.SetTop(cup1, newTop);
                Canvas.SetTop(cup2, newTop);

                await Task.Delay(delayMs);
            }

            // Pozitii finale exacte
            Canvas.SetLeft(cup1, startLeft2);
            Canvas.SetLeft(cup2, startLeft1);
            Canvas.SetTop(cup1, baseTop);
            Canvas.SetTop(cup2, baseTop);
        }

        // ==========================================
        // ETAPA 2: ALEGEREA JUCATORULUI
        // ==========================================
        private async void BtnCup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Parent is not StackPanel parentPanel)
                return;

            BtnCup1.IsEnabled = false;
            BtnCup2.IsEnabled = false;
            BtnCup3.IsEnabled = false;

            // Aflam pozitia logica a paharului ales (1, 2 sau 3)
            double currentX = Canvas.GetLeft(parentPanel);
            int chosenLogicalPosition = currentX == _positions[0] ? 1
                                      : currentX == _positions[1] ? 2 : 3;

            decimal betAmount = decimal.Parse(TxtBet.Text);

            try
            {
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

                    if (result != null)
                    {
                        StackPanel winningCupPanel = GetCupAtPosition(_positions[result.WinningCup - 1]);

                        // Ridicam paharul ales de jucator
                        await LiftCupAsync(parentPanel, true, 220);

                        // Daca paharul ales nu e cel castigator, ridicam si castigatorul
                        if (parentPanel != winningCupPanel && winningCupPanel != null)
                        {
                            await Task.Delay(150);
                            await LiftCupAsync(winningCupPanel, true, 220);
                        }

                        // Aratam bila sub paharul castigator
                        if (winningCupPanel != null)
                        {
                            ShowBallInCup(winningCupPanel);
                        }

                        if (result.IsWin)
                        {
                            TxtStatus.Text = $"FELICITĂRI! CÂȘTIG: +{betAmount * 2:F2} RON";
                            TxtStatus.Foreground = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#00E676"));
                            SetDealerMouth("smile");
                        }
                        else
                        {
                            TxtStatus.Text = "AI PIERDUT! MAI ÎNCEARCĂ O DATĂ.";
                            TxtStatus.Foreground = Brushes.Red;
                            SetDealerMouth("sad");
                        }
                    }

                    RefreshBalance();
                }
                else
                {
                    MessageBox.Show("A apărut o problemă la procesarea pariului.", "Eroare Server");
                    TxtStatus.Text = "EROARE LA PROCESARE.";
                }
            }
            catch
            {
                MessageBox.Show("Eroare de conexiune la serverul de joc.", "Eroare Rețea");
                TxtStatus.Text = "EROARE REȚEA.";
            }

            await Task.Delay(3200);

            ResetBoard();
            SetDealerMouth("neutral");

            TxtStatus.Text = "ALEGE MIZA ȘI APASĂ AMESTECĂ!";
            TxtStatus.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#FFD700"));

            BtnStart.IsEnabled = true;
            TxtBet.IsEnabled = true;
            _isPlaying = false;
        }

        // ==========================================
        // UTILITARE
        // ==========================================

        // Returneaza StackPanel-ul al carui Canvas.Left e cel mai aproape de x
        private StackPanel GetCupAtPosition(double x)
        {
            if (Math.Abs(Canvas.GetLeft(Cup1) - x) < 1) return Cup1;
            if (Math.Abs(Canvas.GetLeft(Cup2) - x) < 1) return Cup2;
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

            // Resetam pozitiile pe X (Canvas.Left)
            Canvas.SetLeft(Cup1, _positions[0]);
            Canvas.SetLeft(Cup2, _positions[1]);
            Canvas.SetLeft(Cup3, _positions[2]);

            // Resetam pozitiile pe Y (Canvas.Top)
            Canvas.SetTop(Cup1, CupTopBase);
            Canvas.SetTop(Cup2, CupTopBase);
            Canvas.SetTop(Cup3, CupTopBase);
        }

        private void BtnLowerBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtBet.Text, out decimal val) && val > 1)
                TxtBet.Text = (val - 1).ToString("F2");
        }

        private void BtnHigherBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtBet.Text, out decimal val))
                TxtBet.Text = (val + 1).ToString("F2");
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
        public string? Message { get; set; }
    }
}