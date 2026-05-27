using LoseBet.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private string _targetWord;
        private int _currentRow = 0;
        private decimal _currentBet = 0;

        private string[] _wordList = { "CARTE", "SOARE", "NOROC", "SPORT", "BANII", "CAZAN", "PIESA", "LUMEA", "JOCURI", "BRAVO" };
        private TextBlock[,] _cells = new TextBlock[6, 5];

        public WordleWindow()
        {
            InitializeComponent();
            GenerateGrid();
            RefreshBalance();
        }

        private async void RefreshBalance()
        {
            // Tragem soldul real direct din server
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"Sold: {balance:F2} RON";
            GameService.NotifyBalanceChanged();
        }

        private void GenerateGrid()
        {
            GridWordle.Children.Clear();
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    Border b = new Border
                    {
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A3C")),
                        BorderThickness = new Thickness(2),
                        Margin = new Thickness(2),
                        Background = Brushes.Transparent
                    };
                    TextBlock txt = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontSize = 32,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    b.Child = txt;
                    GridWordle.Children.Add(b);
                    _cells[r, c] = txt;
                }
            }
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

            // Trimitem miza la server (serverul va scădea banii din baza de date)
            try
            {
                var payload = new { UserId = UserSession.UserId, Amount = -_currentBet };
                var response = await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload); // Folosim endpoint-ul tău de wallet

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Eroare la procesarea mizei pe server.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare de conexiune la server: " + ex.Message);
                return;
            }

            RefreshBalance();

            Random rnd = new Random();
            _targetWord = _wordList[rnd.Next(_wordList.Length)].ToUpper();

            _currentRow = 0;
            GenerateGrid();

            PanelStart.Visibility = Visibility.Collapsed;
            PanelInput.Visibility = Visibility.Visible;
            TxtStatus.Text = "Ai 6 încercări să ghicești cuvântul!";
            TxtStatus.Foreground = Brushes.White;
            TxtGuess.Clear();
            TxtGuess.Focus();
        }

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string guess = TxtGuess.Text.Trim().ToUpper();
            if (guess.Length != 5)
            {
                MessageBox.Show("Cuvântul trebuie să aibă exact 5 litere!");
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                _cells[_currentRow, i].Text = guess[i].ToString();
                Border parent = (Border)_cells[_currentRow, i].Parent;

                if (guess[i] == _targetWord[i])
                {
                    parent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#538D4E"));
                    parent.BorderThickness = new Thickness(0);
                }
                else if (_targetWord.Contains(guess[i]))
                {
                    parent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B59F3B"));
                    parent.BorderThickness = new Thickness(0);
                }
                else
                {
                    parent.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A3C"));
                    parent.BorderThickness = new Thickness(0);
                }
            }

            if (guess == _targetWord)
            {
                decimal winAmount = _currentBet * 3; // Câștigul x3

                // Trimitem câștigul la server
                var payload = new { UserId = UserSession.UserId, Amount = winAmount };
                await _httpClient.PostAsJsonAsync("api/wallet/update-balance", payload);

                TxtStatus.Text = $"GENIAL! Ai câștigat {winAmount:F2} RON!";
                TxtStatus.Foreground = Brushes.Lime;
                RefreshBalance();
                EndGame();
            }
            else if (_currentRow == 5)
            {
                TxtStatus.Text = $"AI PIERDUT! Cuvântul era: {_targetWord}";
                TxtStatus.Foreground = Brushes.Red;
                // Banii au fost luați deja la începutul rundei, deci nu mai scădem nimic acum
                RefreshBalance();
                EndGame();
            }
            else
            {
                _currentRow++;
                TxtGuess.Clear();
                TxtGuess.Focus();
            }
        }

        private void EndGame()
        {
            PanelInput.Visibility = Visibility.Collapsed;
            PanelStart.Visibility = Visibility.Visible;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}