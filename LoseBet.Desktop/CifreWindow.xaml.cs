using LoseBet.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace LoseBet.Desktop
{
    public partial class CifreWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        private decimal _currentBet;
        private int _targetNumber;
        private List<int> _availableNumbers = new List<int>();
        private DispatcherTimer _timer;
        private int _timeLeft;

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
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"Sold: {balance:F2} RON";
            GameService.NotifyBalanceChanged();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            decimal currentRealBalance = await GameService.GetBalanceAsync();

            if (!decimal.TryParse(TxtBet.Text, out decimal bet) || bet <= 0)
            {
                MessageBox.Show("Introdu o miză validă!");
                return;
            }

            if (bet > currentRealBalance)
            {
                MessageBox.Show("Fonduri insuficiente pentru această miză!");
                return;
            }

            _currentBet = bet;

            // Retragem miza prin API de pe server
            try
            {
                var payload = new { UserId = UserSession.UserId, Amount = -_currentBet };
                var response = await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Eroare la procesarea tranzacției pe server.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare rețea: " + ex.Message);
                return;
            }

            RefreshBalance();

            PanelStart.Visibility = Visibility.Collapsed;
            PanelGame.Visibility = Visibility.Visible;
            TxtStatus.Text = "";
            TxtEquation.Clear();

            GenerateGameData();

            _timeLeft = 60;
            TxtTimer.Text = $"⏳ {_timeLeft}s";
            _timer.Start();
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft--;
            TxtTimer.Text = $"⏳ {_timeLeft}s";
            if (_timeLeft <= 0)
            {
                _timer.Stop();
                EndGame(false, "Timpul a expirat!");
            }
        }

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

                if (Math.Abs(result - _targetNumber) < 0.01) // Răspuns exact fix!
                {
                    multiplier = 5; // Câștig x5
                    winMessage = $"EXACT! Ai obținut fix {_targetNumber}! Câștigi x5: ";
                }
                else if (Math.Abs(result - _targetNumber) <= 10) // Foarte aproape (eroare max 10 cifre)
                {
                    multiplier = 2; // Câștig x2
                    winMessage = $"Foarte aproape! Rezultatul tău e {result}. Câștigi x2: ";
                }

                if (multiplier > 0)
                {
                    decimal totalWin = _currentBet * multiplier;

                    // Trimitere câștig la API
                    var payload = new { UserId = UserSession.UserId, Amount = totalWin };
                    await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload);

                    EndGame(true, winMessage + $"{totalWin:F2} RON!");
                }
                else
                {
                    EndGame(false, $"Ai obținut {result}. Prea departe de ținta de {_targetNumber}!");
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
            TxtStatus.Text = msg;
            TxtStatus.Foreground = won ? Brushes.Lime : Brushes.Red;

            PanelGame.Visibility = Visibility.Collapsed;
            PanelStart.Visibility = Visibility.Visible;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            this.Close();
        }
    }
}