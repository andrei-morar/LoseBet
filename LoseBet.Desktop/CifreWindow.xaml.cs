using LoseBet.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LoseBet.Desktop
{
    public partial class CifreWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        private decimal _currentBet;
        private decimal _currentBalance;
        private int _targetNumber;
        private List<int> _availableNumbers = new List<int>();
        private DispatcherTimer _timer;
        private int _timeLeft;

        // Lăcatul de siguranță
        private bool _isClosing = false;

        public CifreWindow()
        {
            InitializeComponent();
            RefreshBalance();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
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
        // BUTOANE MIZĂ RAPIDĂ
        // ==========================================
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
                var payload = new { UserId = UserSession.UserId, Amount = -_currentBet };
                var response = await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload);

                if (!response.IsSuccessStatusCode)
                {
                    SetStatus("EROARE LA PARIERE (SERVER).", "#FF5252");
                    return;
                }
            }
            catch (Exception)
            {
                SetStatus("EROARE DE REȚEA!", "#FF5252");
                return;
            }

            RefreshBalance();

            PanelStart.Visibility = Visibility.Collapsed;
            PanelGame.Visibility = Visibility.Visible;
            SetStatus("CALCULEAZĂ REPEDE!", "#FFD700");
            TxtEquation.Clear();

            GenerateGameData();

            _timeLeft = 60;
            TxtTimer.Text = _timeLeft.ToString();
            _timer.Start();

            TxtEquation.Focus();
        }

        private void GenerateGameData()
        {
            Random r = new Random();
            _targetNumber = r.Next(101, 999);
            TxtTarget.Text = _targetNumber.ToString();

            _availableNumbers.Clear();
            for (int i = 0; i < 4; i++) _availableNumbers.Add(r.Next(1, 11));

            int[] bigNumbers = { 25, 50, 75, 100 };
            _availableNumbers.Add(bigNumbers[r.Next(0, 4)]);
            _availableNumbers.Add(bigNumbers[r.Next(0, 4)]);

            ListNumbers.ItemsSource = null;
            ListNumbers.ItemsSource = _availableNumbers;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timeLeft--;
            TxtTimer.Text = _timeLeft.ToString();

            if (_timeLeft <= 10)
            {
                TxtTimer.Foreground = Brushes.Red;
            }

            if (_timeLeft <= 0)
            {
                _timer.Stop();
                EndGame(false, "TIMPUL A EXPIRAT!");
            }
        }

        // ==========================================
        // VERIFICARE ECUAȚIE
        // ==========================================
        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string input = TxtEquation.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            var matches = Regex.Matches(input, @"\d+");
            List<int> usedNumbers = new List<int>();
            foreach (Match m in matches) usedNumbers.Add(int.Parse(m.Value));

            var availableCopy = new List<int>(_availableNumbers);
            foreach (int num in usedNumbers)
            {
                if (availableCopy.Contains(num))
                {
                    availableCopy.Remove(num);
                }
                else
                {
                    MessageBox.Show($"Ai folosit numărul {num} care nu se află în lista primită!");
                    return;
                }
            }

            try
            {
                DataTable dt = new DataTable();
                var resultObj = dt.Compute(input, "");
                double result = Convert.ToDouble(resultObj);

                _timer.Stop();
                decimal multiplier = 0;
                string winMessage = "";

                if (Math.Abs(result - _targetNumber) < 0.01)
                {
                    multiplier = 5;
                    winMessage = $"EXACT! Ai obținut fix {_targetNumber}! Câștigi x5: ";
                }
                else if (Math.Abs(result - _targetNumber) <= 10)
                {
                    multiplier = 2;
                    winMessage = $"FOARTE APROAPE! ({result}). Câștigi x2: ";
                }

                if (multiplier > 0)
                {
                    decimal totalWin = _currentBet * multiplier;

                    try
                    {
                        var payload = new { UserId = UserSession.UserId, Amount = totalWin };
                        await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload);
                    }
                    catch { }

                    EndGame(true, winMessage + $"{totalWin:F2} RON!");
                }
                else
                {
                    EndGame(false, $"GREȘIT! Rezultat: {result}. Ținta era {_targetNumber}.");
                }
            }
            catch
            {
                MessageBox.Show("Ecuația introdusă nu este validă din punct de vedere matematic!");
            }
        }

        private void EndGame(bool won, string msg)
        {
            _timer.Stop();
            RefreshBalance();
            SetStatus(msg, won ? "#00E676" : "#FF5252");

            TxtTimer.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));

            PanelGame.Visibility = Visibility.Collapsed;
            PanelStart.Visibility = Visibility.Visible;
        }

        private void SetStatus(string text, string hexColor)
        {
            TxtStatus.Text = text;
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }

        // AICI AM APLICAT LĂCATUL (și oprirea timerului)
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            _isClosing = true;

            if (sender is Button btn) btn.IsEnabled = false;

            _timer?.Stop();

            DashboardWindow dashboard = new DashboardWindow("", "");
            dashboard.Show();
            this.Close();
        }
    }
}