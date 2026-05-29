using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LoseBet.Desktop
{
    public partial class MinesWindow : Window
    {
        private int _totalMines = 3;
        private decimal _betAmount = 0;
        private bool _isGameActive = false;
        private int _currentSessionId = 0;
        private int _revealedCount = 0;
        private decimal _currentBalance = 0;

        // Lăcatul pentru a preveni dublu-click pe închidere
        private bool _isClosing = false;

        // Culori pentru animațiile pe grilă
        private static readonly Color ColorTileDefault = (Color)ColorConverter.ConvertFromString("#1C2B3A");
        private static readonly Color ColorTileSafe = (Color)ColorConverter.ConvertFromString("#0D3320");
        private static readonly Color ColorTileBomb = (Color)ColorConverter.ConvertFromString("#3A0A0A");

        public MinesWindow()
        {
            InitializeComponent();
            RefreshBalance();
            GenerateBoard();
            UpdateMineCounter();
        }

        // ==========================================
        // BALANCE
        // ==========================================
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
                // Ignoră erorile silențios
            }
        }

        // ==========================================
        // GENERARE TABLA 
        // ==========================================
        private void GenerateBoard()
        {
            MinesGrid.Children.Clear();
            for (int i = 0; i < 25; i++)
            {
                Button btn = new Button
                {
                    Tag = i,
                    Style = (Style)FindResource("TileStyle"),
                    IsEnabled = false
                };
                btn.Click += Tile_Click;
                MinesGrid.Children.Add(btn);
            }
        }

        private void ResetBoard()
        {
            _revealedCount = 0;
            TxtRevealed.Text = "0";
            MultiplierText.Text = "—";
            TxtPotential.Text = "0.00 RON";

            foreach (Button btn in MinesGrid.Children)
            {
                btn.Content = "";
                btn.Background = new SolidColorBrush(ColorTileDefault);
                btn.IsEnabled = false;
                btn.Opacity = 1;
            }
        }

        // ==========================================
        // QUICK BET BUTTONS (1x, 2x, Max etc.)
        // ==========================================
        private void QuickBet_Click(object sender, RoutedEventArgs e)
        {
            if (_isGameActive) return;
            var tag = (sender as Button)?.Tag?.ToString();
            if (tag == null) return;

            if (tag == "max")
            {
                BetInput.Text = _currentBalance.ToString("F2");
                return;
            }

            if (decimal.TryParse(BetInput.Text, out decimal cur) &&
                decimal.TryParse(tag, out decimal factor))
            {
                decimal newVal = Math.Round(cur * factor, 2);
                if (newVal <= 0) newVal = 1;
                BetInput.Text = newVal.ToString("F2");
            }
        }

        private void UpdateMineCounter()
        {
            if (TxtMineCount == null) return;

            if (MinesCountCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                TxtMineCount.Text = item.Tag.ToString();
            }
        }

        private void MinesCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMineCounter();
        }

        // ==========================================
        // START JOC
        // ==========================================
        private async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(BetInput.Text, out _betAmount) || _betAmount <= 0)
            {
                SetStatus("SETEAZĂ O MIZĂ VALIDĂ!", "#FF5252");
                return;
            }

            if (MinesCountCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                _totalMines = int.Parse(selectedItem.Tag.ToString()!);
            }

            BtnPlay.IsEnabled = false;
            BtnCashout.IsEnabled = false;
            BetInput.IsEnabled = false;
            MinesCountCombo.IsEnabled = false;

            ResetBoard();
            SetStatus("SE PREGĂTEȘTE TABLA...", "#FFD700");

            var result = await GameService.StartMinesAsync(_betAmount, _totalMines);

            if (result == null)
            {
                SetStatus("EROARE SAU FONDURI INSUFICIENTE!", "#FF5252");
                BtnPlay.IsEnabled = true;
                BetInput.IsEnabled = true;
                MinesCountCombo.IsEnabled = true;
                return;
            }

            _currentSessionId = result.SessionId;
            _isGameActive = true;

            MultiplierText.Text = "1.00x";
            TxtPotential.Text = $"{_betAmount:F2} RON";

            BtnCashout.IsEnabled = true;
            BtnCashout.Content = $"💰  CASHOUT ({_betAmount:F2} RON)";

            foreach (Button btn in MinesGrid.Children)
            {
                btn.IsEnabled = true;
            }

            RefreshBalance();
            SetStatus("ALEGE O CĂSUȚĂ!", "White");
        }

        // ==========================================
        // CLICK PE CĂSUȚĂ
        // ==========================================
        private async void Tile_Click(object sender, RoutedEventArgs e)
        {
            if (!_isGameActive) return;

            if (sender is not Button btn || btn.Tag == null) return;

            int position = (int)btn.Tag;
            btn.IsEnabled = false;

            var result = await GameService.PickMineAsync(_currentSessionId, position);

            if (result == null)
            {
                SetStatus("EROARE DE CONEXIUNE!", "#FF5252");
                btn.IsEnabled = true;
                return;
            }

            if (result.HitBomb)
            {
                AnimateTile(btn, ColorTileBomb);
                btn.Content = "💣";
                btn.FontSize = 26;
                EndGame(false, $"Ai lovit o mină! Pierdere: {_betAmount:F2} RON.");
            }
            else
            {
                _revealedCount++;
                TxtRevealed.Text = _revealedCount.ToString();

                AnimateTile(btn, ColorTileSafe);
                btn.Content = "💎";
                btn.FontSize = 26;

                decimal mult = result.CurrentMultiplier;
                decimal liveProfit = _betAmount * mult;

                MultiplierText.Text = $"{mult:F2}x";
                TxtPotential.Text = $"{liveProfit:F2} RON";
                BtnCashout.Content = $"💰  CASHOUT ({liveProfit:F2} RON)";

                SetStatus($"BINE! +{mult:F2}x  —  CONTINUĂ SAU RETRAGE!", "#00E676");
            }
        }

        // ==========================================
        // CASHOUT
        // ==========================================
        private async void BtnCashout_Click(object sender, RoutedEventArgs e)
        {
            if (!_isGameActive) return;

            BtnCashout.IsEnabled = false;

            decimal winAmount = await GameService.CashoutMinesAsync(_currentSessionId);

            if (winAmount > 0)
            {
                EndGame(true, $"Ai retras cu succes: {winAmount:F2} RON! 🎉");
            }
            else
            {
                SetStatus("EROARE LA CASHOUT!", "#FF5252");
                BtnCashout.IsEnabled = true;
            }
        }

        // ==========================================
        // SFÂRȘIT JOC
        // ==========================================
        private void EndGame(bool won, string message)
        {
            _isGameActive = false;

            foreach (Button btn in MinesGrid.Children)
            {
                btn.IsEnabled = false;
            }

            BtnPlay.IsEnabled = true;
            BtnCashout.IsEnabled = false;
            BtnCashout.Content = "💰  CASHOUT";
            BetInput.IsEnabled = true;
            MinesCountCombo.IsEnabled = true;

            if (won)
            {
                SetStatus("FELICITĂRI! AI CÂȘTIGAT! 🏆", "#00E676");
                MultiplierText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676"));
            }
            else
            {
                SetStatus("💥 AI LOVIT O MINĂ! GHINION!", "#FF5252");
                MultiplierText.Text = "0.00x";
                MultiplierText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5252"));
            }

            RefreshBalance();
            MessageBox.Show(message, won ? "Câștig! 🎉" : "Boom! 💣", MessageBoxButton.OK, won ? MessageBoxImage.Information : MessageBoxImage.Warning);
            MultiplierText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
        }

        // ==========================================
        // UTILITARE & ANIMAȚII
        // ==========================================
        private void SetStatus(string text, string hexColor)
        {
            TxtStatus.Text = text;
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }

        private void AnimateTile(Button btn, Color targetColor)
        {
            var anim = new ColorAnimation
            {
                From = Colors.White,
                To = targetColor,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };
            var brush = new SolidColorBrush(Colors.White);
            btn.Background = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }

        // AICI ESTE LĂCATUL APLICAT
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