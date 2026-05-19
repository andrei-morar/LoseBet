using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class MinesWindow : Window
    {
        private int _totalMines = 3;
        private decimal _betAmount = 0;
        private bool _isGameActive = false;
        private int _currentSessionId = 0;

        public MinesWindow()
        {
            InitializeComponent();
            RefreshBalance();
            GenerateBoard();
        }

        private async void RefreshBalance()
        {
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"Balanță: {balance:F2} RON";

            // ADAUGĂ LINIA ASTA LA FINAL:
            GameService.NotifyBalanceChanged();
        }

        private void GenerateBoard()
        {
            MinesGrid.Children.Clear();
            for (int i = 0; i < 25; i++)
            {
                Button btn = new Button
                {
                    Tag = i,
                    Margin = new Thickness(4),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30")),
                    Foreground = Brushes.White,
                    FontSize = 24,
                    BorderThickness = new Thickness(0),
                    IsEnabled = false
                };
                btn.Click += Tile_Click;
                MinesGrid.Children.Add(btn);
            }
        }

        private async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(BetInput.Text, out _betAmount) || _betAmount <= 0)
            {
                MessageBox.Show("Miza trebuie să fie un număr pozitiv!");
                return;
            }

            _totalMines = int.Parse(((ComboBoxItem)MinesCountCombo.SelectedItem).Content.ToString());

            BtnPlay.IsEnabled = false;

            // Trimitem cererea de Start Mines la API
            var result = await GameService.StartMinesAsync(_betAmount, _totalMines);

            if (result == null)
            {
                MessageBox.Show("Eroare sau fonduri insuficiente!");
                BtnPlay.IsEnabled = true;
                return;
            }

            // Salvăm ID-ul sesiunii venite de la server
            _currentSessionId = result.SessionId;

            _isGameActive = true;
            MultiplierText.Text = "1.00x";

            BtnCashout.IsEnabled = true;
            BtnCashout.Content = "CASHOUT";
            BetInput.IsEnabled = false;
            MinesCountCombo.IsEnabled = false;

            RefreshBalance(); // Banii s-au dus pe pariu

            // Resetăm tabla
            foreach (Button btn in MinesGrid.Children)
            {
                btn.Content = "";
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
                btn.IsEnabled = true;
            }
        }

        private async void Tile_Click(object sender, RoutedEventArgs e)
        {
            if (!_isGameActive) return;

            Button btn = sender as Button;
            int position = (int)btn.Tag;

            btn.IsEnabled = false; // Prevenim dublu click până răspunde serverul

            // Cerem de la server rezultatul pentru căsuța asta
            var result = await GameService.PickMineAsync(_currentSessionId, position);

            if (result == null)
            {
                MessageBox.Show("Eroare de conexiune la server.");
                btn.IsEnabled = true;
                return;
            }

            if (result.HitBomb)
            {
                // BOOM!
                btn.Background = Brushes.Crimson;
                btn.Content = "💣";
                EndGame(false, "Ai lovit o mină!");
            }
            else
            {
                // SAFE!
                btn.Background = Brushes.SeaGreen;
                btn.Content = "💎";

                MultiplierText.Text = $"{result.CurrentMultiplier:F2}x";

                decimal liveProfit = _betAmount * result.CurrentMultiplier;
                BtnCashout.Content = $"RETRAGE ({liveProfit:F2})";

                // Opțional, poți opri jocul automat dacă a deschis toate căsuțele sigure
                // Dar logic ar trebui lăsat să dea el Cashout
            }
        }

        private async void BtnCashout_Click(object sender, RoutedEventArgs e)
        {
            if (_isGameActive)
            {
                BtnCashout.IsEnabled = false;

                // Trimitem cererea de cashout
                decimal winAmount = await GameService.CashoutMinesAsync(_currentSessionId);

                if (winAmount > 0)
                {
                    EndGame(true, $"Ai retras cu succes: {winAmount:F2} RON!");
                }
                else
                {
                    MessageBox.Show("Eroare la Cashout!");
                    BtnCashout.IsEnabled = true;
                }
            }
        }

        private void EndGame(bool won, string message)
        {
            _isGameActive = false;
            BtnPlay.IsEnabled = true;
            BtnCashout.IsEnabled = false;
            BetInput.IsEnabled = true;
            MinesCountCombo.IsEnabled = true;

            // Blocăm restul butoanelor
            foreach (Button btn in MinesGrid.Children)
            {
                btn.IsEnabled = false;
            }

            RefreshBalance();
            MessageBox.Show(message, won ? "Câștig!" : "Bust");
        }
    }
}