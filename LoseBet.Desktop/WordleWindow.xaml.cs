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
    public partial class WordleWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        private string _targetWord = "";
        private int _currentRow = 0;
        private decimal _currentBet = 0;
        private decimal _currentBalance = 0;

        // Lăcatul de siguranță
        private bool _isClosing = false;

        private string[] _wordList = { "CARTE", "SOARE", "NOROC", "SPORT", "BANII", "CAZAN", "PIESA", "LUMEA", "JOCURI", "BRAVO", "TABLA", "BINGO", "MASCA" };
        private TextBlock[,] _cells = new TextBlock[6, 5];

        public WordleWindow()
        {
            InitializeComponent();
            GenerateGrid();
            RefreshBalance();
        }

        private async void RefreshBalance()
        {
            try
            {
                decimal balance = await GameService.GetBalanceAsync();
                _currentBalance = balance;
                TxtBalance.Text = $"{balance:F2} RON";
                GameService.NotifyBalanceChanged();
            }
            catch
            {
                // Ignoram erorile silențios
            }
        }

        // ==========================================
        // DESIGN GRILA WORDLE
        // ==========================================
        private void GenerateGrid()
        {
            GridWordle.Children.Clear();
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    Border b = new Border
                    {
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2B35")),
                        BorderThickness = new Thickness(2),
                        Margin = new Thickness(4),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14151A")),
                        CornerRadius = new CornerRadius(8)
                    };

                    TextBlock txt = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontSize = 32,
                        FontWeight = FontWeights.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    b.Child = txt;
                    GridWordle.Children.Add(b);
                    _cells[r, c] = txt;
                }
            }
        }

        // ==========================================
        // START JOC (PARIERE)
        // ==========================================
        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtBet.Text, out decimal bet) || bet <= 0)
            {
                SetStatus("INTRODU O MIZĂ VALIDĂ!", "#FF5252");
                return;
            }

            if (bet > _currentBalance)
            {
                SetStatus("FONDURI INSUFICIENTE!", "#FF5252");
                return;
            }

            _currentBet = bet;

            try
            {
                // Trimitem miza la server (scădem banii)
                var payload = new { UserId = UserSession.UserId, Amount = -_currentBet };
                var response = await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload);

                if (!response.IsSuccessStatusCode)
                {
                    SetStatus("EROARE LA PARIERE!", "#FF5252");
                    return;
                }
            }
            catch (Exception)
            {
                SetStatus("EROARE REȚEA!", "#FF5252");
                return;
            }

            RefreshBalance();

            Random rnd = new Random();
            _targetWord = _wordList[rnd.Next(_wordList.Length)].ToUpper();

            _currentRow = 0;
            GenerateGrid();

            PanelStart.Visibility = Visibility.Collapsed;
            PanelInput.Visibility = Visibility.Visible;

            SetStatus($"AI 6 ÎNCERCĂRI! (Câștig: {_currentBet * 3:F2} RON)", "#FFD700");

            TxtGuess.Clear();
            TxtGuess.Focus();
        }

        // ==========================================
        // LOGICA DE JOC (GHICIRE)
        // ==========================================
        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string guess = TxtGuess.Text.Trim().ToUpper();
            if (guess.Length != 5)
            {
                SetStatus("CUVÂNTUL TREBUIE SĂ AIBĂ 5 LITERE!", "#FF5252");
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                _cells[_currentRow, i].Text = guess[i].ToString();
                Border parent = (Border)_cells[_currentRow, i].Parent;

                if (guess[i] == _targetWord[i])
                {
                    parent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E7A34"));
                    parent.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676"));
                }
                else if (_targetWord.Contains(guess[i]))
                {
                    parent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B57C00"));
                    parent.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
                }
                else
                {
                    parent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111115"));
                    parent.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
                    _cells[_currentRow, i].Foreground = Brushes.Gray;
                }
            }

            if (guess == _targetWord)
            {
                decimal winAmount = _currentBet * 3;

                try
                {
                    var payload = new { UserId = UserSession.UserId, Amount = winAmount };
                    await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload);
                }
                catch { }

                SetStatus($"GENIAL! AI CÂȘTIGAT {winAmount:F2} RON! 🎉", "#00E676");
                RefreshBalance();
                EndGame();
            }
            else if (_currentRow == 5)
            {
                SetStatus($"AI PIERDUT! CUVÂNTUL ERA: {_targetWord}", "#FF5252");
                RefreshBalance();
                EndGame();
            }
            else
            {
                _currentRow++;
                TxtGuess.Clear();
                TxtGuess.Focus();
                SetStatus($"ÎNCERCAREA {_currentRow + 1} DIN 6", "#FFD700");
            }
        }

        private void EndGame()
        {
            PanelInput.Visibility = Visibility.Collapsed;
            PanelStart.Visibility = Visibility.Visible;
        }

        // ==========================================
        // UTILAJE INTERFAȚĂ
        // ==========================================
        private void TxtGuess_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BtnSubmit != null)
            {
                BtnSubmit.IsEnabled = TxtGuess.Text.Trim().Length == 5;
            }
        }

        private void QuickBet_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag?.ToString();
            if (tag == null) return;

            if (tag == "max")
            {
                TxtBet.Text = _currentBalance.ToString("F2");
                return;
            }

            if (decimal.TryParse(TxtBet.Text, out decimal cur) &&
                decimal.TryParse(tag, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal factor))
            {
                decimal nv = Math.Round(cur * factor, 2);
                TxtBet.Text = (nv < 1 ? 1 : nv).ToString("F2");
            }
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

        private void SetStatus(string text, string hexColor)
        {
            TxtStatus.Text = text;
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }

        // AICI AM APLICAT LĂCATUL
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            _isClosing = true;

            if (sender is Button btn) btn.IsEnabled = false;

            DashboardWindow dashboard = new DashboardWindow("", "");
            dashboard.Show();
            this.Close();
        }
    }
}