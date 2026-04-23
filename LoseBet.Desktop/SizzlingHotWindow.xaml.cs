using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class SizzlingHotWindow : Window
    {
        private decimal _balance;
        private string _username;
        private decimal _currentWin;
        private Random _random = new Random();

        private string[] _symbols = { "7️⃣", "⭐", "🍉", "🍇", "🍊", "🍋", "🍒", "🍒" }; // Cireșe duble pt șansă mai mare
        private TextBlock[,] _matrix = new TextBlock[3, 5];

        public SizzlingHotWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            decimal.TryParse(balance, out _balance);
            UpdateUI();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    // AICI ERA PROBLEMA! Am adăugat Foreground = Brushes.White
                    var tb = new TextBlock
                    {
                        FontSize = 50,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = "🍇"
                    };

                    _matrix[r, c] = tb;
                    SlotsGrid.Children.Add(tb);
                }
            }
        }

        private async void BtnSpin_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtBet.Text, out decimal bet) || bet > _balance || bet <= 0) return;

            _balance -= bet;
            UpdateUI();
            BtnSpin.IsEnabled = false;
            PanelDublaj.Visibility = Visibility.Collapsed;

            // Animație
            for (int i = 0; i < 15; i++)
            {
                foreach (var tb in _matrix) tb.Text = _symbols[_random.Next(_symbols.Length)];
                await Task.Delay(60);
            }

            CheckWins(bet);
        }

        private void CheckWins(decimal bet)
        {
            decimal win = 0;
            // Cele 5 linii: Rând 0, Rând 1, Rând 2, V (0,1,2,1,0), V-întors (2,1,0,1,2)
            int[][] lines = {
                new int[] {0,0, 0,1, 0,2, 0,3, 0,4}, // Linia 1
                new int[] {1,0, 1,1, 1,2, 1,3, 1,4}, // Linia 2
                new int[] {2,0, 2,1, 2,2, 2,3, 2,4}, // Linia 3
                new int[] {0,0, 1,1, 2,2, 1,3, 0,4}, // Linia 4
                new int[] {2,0, 1,1, 0,2, 1,3, 2,4}  // Linia 5
            };

            foreach (var line in lines)
            {
                string s1 = _matrix[line[0], line[1]].Text;
                int count = 1;
                for (int i = 2; i < 10; i += 2)
                {
                    if (_matrix[line[i], line[i + 1]].Text == s1) count++;
                    else break;
                }

                if (count >= 3 || (s1 == "🍒" && count >= 2))
                    win += CalculateSymbolWin(s1, count, bet);
            }

            // Scatter (Steaua) - plătește oriunde
            int stars = 0;
            foreach (var tb in _matrix) if (tb.Text == "⭐") stars++;
            if (stars >= 3) win += bet * stars * 2;

            if (win > 0)
            {
                _currentWin = win;
                TxtStatus.Text = $"CÂȘTIG: {win} RON! DUBLĂM?";
                PanelDublaj.Visibility = Visibility.Visible;
            }
            else
            {
                TxtStatus.Text = "MAI ÎNCEARCĂ!";
                BtnSpin.IsEnabled = true;
            }
        }

        private decimal CalculateSymbolWin(string sym, int count, decimal bet)
        {
            decimal mult = count == 5 ? 100 : (count == 4 ? 20 : 5);
            if (sym == "7️⃣") mult *= 10;
            return bet * mult;
        }

        // --- Logica Dublaj & Colectare ---
        private void BtnGamble_Click(object sender, RoutedEventArgs e)
        {
            if (_random.Next(0, 2) == 0) { _currentWin *= 2; TxtStatus.Text = $"DUBLAT: {_currentWin} RON"; }
            else { _currentWin = 0; PanelDublaj.Visibility = Visibility.Collapsed; BtnSpin.IsEnabled = true; TxtStatus.Text = "AI PIERDUT!"; }
        }

        private void BtnCollect_Click(object sender, RoutedEventArgs e)
        {
            _balance += _currentWin;
            _currentWin = 0;
            UpdateUI();
            PanelDublaj.Visibility = Visibility.Collapsed;
            BtnSpin.IsEnabled = true;
        }

        private void UpdateUI() => TxtBalance.Text = $"Sold: {_balance:0.00} RON";
        private void BtnBack_Click(object sender, RoutedEventArgs e) { new SlotsLobbyWindow(_username, _balance.ToString()).Show(); this.Close(); }
    }
}