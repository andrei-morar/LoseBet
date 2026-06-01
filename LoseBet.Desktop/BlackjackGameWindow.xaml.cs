using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace LoseBet.Desktop
{
    public partial class BlackjackGameWindow : Window
    {
        private decimal _balance;
        private string? _username;
        private decimal _currentBet = 0;
        private bool _isVip = false; // FLAG PENTRU MASA VIP

        private List<string> _deck = new List<string>();
        private List<string> _playerCards = new List<string>();
        private List<string> _dealerCards = new List<string>();
        private Random _random = new Random();
        private Button? _selectedChip = null;

        // Am adăugat isVip în constructor!
        public BlackjackGameWindow(string? username, string? balance, bool isVip = false)
        {
            InitializeComponent();
            _username = username ?? "Guest";
            decimal.TryParse(balance ?? "0", out _balance);
            _isVip = isVip;

            ApplyTableTheme();
            GenerateChips();
            UpdateBalanceDisplay();
            UpdateBetDisplay();
        }

        private void BtnChipsLeft_Click(object sender, RoutedEventArgs e)
        {
            var sv = this.FindName("ChipsScroll") as ScrollViewer;
            sv?.LineLeft();
        }

        private void BtnChipsRight_Click(object sender, RoutedEventArgs e)
        {
            var sv = this.FindName("ChipsScroll") as ScrollViewer;
            sv?.LineRight();
        }

        // ================= TEMATICĂ ȘI JETOANE =================

        private void ApplyTableTheme()
        {
            if (_isVip)
            {
                TxtGameTitle.Text = "LOSEBET VIP BLACKJACK";
                // Schimbăm fundalul în Roșu închis pentru VIP
                BgGradientCenter.Color = (Color)ColorConverter.ConvertFromString("#4A0000");
                BgGradientEdge.Color = (Color)ColorConverter.ConvertFromString("#110000");
            }
            else
            {
                TxtGameTitle.Text = "LOSEBET CLASSIC BLACKJACK";
                // Păstrăm verdele clasic
                BgGradientCenter.Color = (Color)ColorConverter.ConvertFromString("#005522");
                BgGradientEdge.Color = (Color)ColorConverter.ConvertFromString("#001105");
            }
        }

        private void GenerateChips()
        {
            ChipsPanel.Children.Clear();

            // Mizele tale pentru VIP vs Classic
            // Use requested fixed chip values for betting (single-select)
            int[] chipValues = _isVip
                ? new int[] { 500, 750, 1000, 2000, 3000, 5000, 7500, 10000, 25000, 50000 }
                : new int[] { 10, 20, 30, 40, 50, 60, 80, 100, 250, 400 };

            string[] chipColors = _isVip
                ? new string[] { "#8B0000", "#4B0082", "#2F4F4F", "#000080", "#B8860B", "#1A1A1A", "#800000", "#FF4500", "#000000" }
                : new string[] { "#808080", "#0000FF", "#FF0000", "#008000", "#000000" };

            for (int i = 0; i < chipValues.Length; i++)
            {
                int val = chipValues[i];
                Button chip = new Button();

                // Dacă e peste 1000, scriem "1k", "5k" ca să încapă frumos pe jeton
                chip.Content = val >= 1000 ? (val / 1000).ToString() + "k" : val.ToString();
                chip.Tag = val;
                chip.Style = (Style)FindResource("ChipButtonStyle");
                chip.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(chipColors[i % chipColors.Length]));

                chip.Click += Chip_Click;
                ChipsPanel.Children.Add(chip);
            }
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            int chipValue = (int)btn.Tag;

            if (chipValue > _balance)
            {
                MessageBox.Show("Nu ai suficienți bani pentru această miză!", "Fonduri insuficiente");
                return;
            }

            // Single-selection behavior: set bet to this chip value once and disable other chips
            _currentBet = chipValue;
            UpdateBetDisplay();

            // mark selected
            _selectedChip = btn;
            foreach (var child in ChipsPanel.Children)
            {
                if (child is Button b)
                {
                    b.IsEnabled = false;
                    b.Opacity = b == _selectedChip ? 1.0 : 0.6;
                }
            }
        }

        private void BtnClearBet_Click(object sender, RoutedEventArgs e)
        {
            _currentBet = 0;
            UpdateBetDisplay();

            // Re-enable chips
            foreach (var child in ChipsPanel.Children)
            {
                if (child is Button b)
                {
                    b.IsEnabled = true;
                    b.Opacity = 1.0;
                }
            }
            _selectedChip = null;
        }

        private void UpdateBalanceDisplay() => TxtBalance.Text = $"Sold: {_balance:0.00} RON";
        private void UpdateBetDisplay() => TxtCurrentBet.Text = $" (Miza: {_currentBet} RON)";


        // ================= LOGICA DE JOC =================

        private void InitializeDeck()
        {
            _deck = new List<string>();
            string[] suits = { "♥", "♦", "♣", "♠" };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (var suit in suits)
            {
                foreach (var val in values)
                {
                    _deck.Add($"{val}{suit}");
                }
            }
            _deck = _deck.OrderBy(x => _random.Next()).ToList();
        }

        private string DrawCard()
        {
            if (_deck == null || _deck.Count == 0)
            {
                InitializeDeck();
            }
            string card = _deck[0];
            _deck.RemoveAt(0);
            return card;
        }

        private int CalculateScore(List<string> hand)
        {
            int score = 0;
            int aces = 0;

            foreach (string card in hand)
            {
                string value = card.Substring(0, card.Length - 1);
                if (value == "J" || value == "Q" || value == "K") score += 10;
                else if (value == "A") { score += 11; aces++; }
                else score += int.Parse(value);
            }

            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }

            return score;
        }

        private UIElement CreateCardVisual(string cardStr, bool hidden = false)
        {
            Border b = new Border
            {
                Width = 80,
                Height = 115,
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(5),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 5, ShadowDepth = 2, Opacity = 0.5 }
            };

            if (hidden)
            {
                b.Background = new SolidColorBrush(Color.FromRgb(0, 34, 102));
                b.BorderBrush = Brushes.White;
                b.BorderThickness = new Thickness(2);
            }
            else
            {
                bool isRed = cardStr.Contains("♥") || cardStr.Contains("♦");
                TextBlock txt = new TextBlock
                {
                    Text = cardStr,
                    FontSize = 26,
                    FontWeight = FontWeights.Bold,
                    Foreground = isRed ? Brushes.Red : Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                b.Child = txt;
            }
            return b;
        }

        private void AnimateCardFlying(UIElement card, bool isPlayer)
        {
            TransformGroup group = new TransformGroup();
            TranslateTransform trans = new TranslateTransform();
            ScaleTransform scale = new ScaleTransform(0.5, 0.5);
            RotateTransform rot = new RotateTransform(isPlayer ? 360 : -360);

            trans.X = 400;
            trans.Y = isPlayer ? -150 : 150;

            group.Children.Add(scale);
            group.Children.Add(rot);
            group.Children.Add(trans);

            card.RenderTransformOrigin = new Point(0.5, 0.5);
            card.RenderTransform = group;

            DoubleAnimation moveX = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(0.4), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            DoubleAnimation moveY = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(0.4), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            DoubleAnimation scaleAnim = new DoubleAnimation { To = 1, Duration = TimeSpan.FromSeconds(0.4), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            DoubleAnimation rotAnim = new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(0.4), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            DoubleAnimation fade = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.2) };

            trans.BeginAnimation(TranslateTransform.XProperty, moveX);
            trans.BeginAnimation(TranslateTransform.YProperty, moveY);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            rot.BeginAnimation(RotateTransform.AngleProperty, rotAnim);
            card.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private async Task DealCardToPlayer(string card)
        {
            _playerCards.Add(card);
            var visual = CreateCardVisual(card, false);
            PanelPlayerCards.Children.Add(visual);

            AnimateCardFlying(visual, true);
            TxtPlayerScore.Text = $"Scorul tău: {CalculateScore(_playerCards)}";

            await Task.Delay(400);
        }

        private async Task DealCardToDealer(string card, bool hidden)
        {
            _dealerCards.Add(card);
            var visual = CreateCardVisual(card, hidden);
            PanelDealerCards.Children.Add(visual);

            AnimateCardFlying(visual, false);

            if (!hidden)
                TxtDealerScore.Text = $"Cărțile Dealerului: {CalculateScore(new List<string> { card })}";

            await Task.Delay(400);
        }

        private async void BtnDeal_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBet <= 0)
            {
                MessageBox.Show("Adaugă miza folosind jetoanele!", "Miză zero");
                return;
            }

            int minBet = _isVip ? 500 : 10;
            if (_currentBet < minBet)
            {
                MessageBox.Show($"Miza minimă la această masă este de {minBet} RON!", "Atenție");
                return;
            }

            _balance -= _currentBet;
            UpdateBalanceDisplay();

            InitializeDeck();
            _playerCards.Clear();
            _dealerCards.Clear();
            PanelPlayerCards.Children.Clear();
            PanelDealerCards.Children.Clear();
            TxtDealerScore.Text = "Cărțile Dealerului: ?";

            PanelBetting.Visibility = Visibility.Collapsed;
            TxtStatus.Text = "Se împart cărțile...";
            TxtStatus.Foreground = Brushes.White;

            await DealCardToPlayer(DrawCard());
            await DealCardToDealer(DrawCard(), false);
            await DealCardToPlayer(DrawCard());
            await DealCardToDealer(DrawCard(), true);

            PanelActions.Visibility = Visibility.Visible;
            TxtStatus.Text = "Hit (Trage) sau Stand (Stai)?";
            TxtStatus.Foreground = Brushes.Gold;

            if (CalculateScore(_playerCards) == 21) EndGame();
        }

        private async void BtnHit_Click(object sender, RoutedEventArgs e)
        {
            BtnHit.IsEnabled = false; BtnStand.IsEnabled = false;

            await DealCardToPlayer(DrawCard());

            if (CalculateScore(_playerCards) > 21)
            {
                EndGame();
            }
            else
            {
                BtnHit.IsEnabled = true; BtnStand.IsEnabled = true;
            }
        }

        private async void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;

            PanelDealerCards.Children.RemoveAt(1);
            var revealedCard = CreateCardVisual(_dealerCards[1], false);
            PanelDealerCards.Children.Insert(1, revealedCard);

            TxtDealerScore.Text = $"Cărțile Dealerului: {CalculateScore(_dealerCards)}";
            await Task.Delay(800);

            while (CalculateScore(_dealerCards) < 17)
            {
                await DealCardToDealer(DrawCard(), false);
                TxtDealerScore.Text = $"Cărțile Dealerului: {CalculateScore(_dealerCards)}";
            }

            EndGame();
        }

        private void EndGame()
        {
            BtnHit.IsEnabled = true;
            BtnStand.IsEnabled = true;

            int pScore = CalculateScore(_playerCards);
            int dScore = CalculateScore(_dealerCards);

            if (pScore <= 21 && (dScore > 21 || pScore > dScore))
            {
                decimal win = _currentBet * 2;
                if (pScore == 21 && _playerCards.Count == 2) win = _currentBet * 2.5m;

                _balance += win;
                UpdateBalanceDisplay();

                TxtWinAmount.Text = $"+ {win:0.00} RON";
                LanseazaArtificii();
            }
            else
            {
                if (pScore > 21)
                {
                    TxtStatus.Text = "BUST! Ai depășit 21. Ai pierdut miza.";
                    TxtStatus.Foreground = Brushes.Tomato;
                }
                else if (pScore == dScore)
                {
                    _balance += _currentBet;
                    UpdateBalanceDisplay();
                    TxtStatus.Text = "EGALITATE (PUSH). Miza returnată.";
                    TxtStatus.Foreground = Brushes.LightBlue;
                }
                else
                {
                    TxtStatus.Text = "DEALERUL CÂȘTIGĂ!";
                    TxtStatus.Foreground = Brushes.Tomato;
                }

                _currentBet = 0; // Resetăm miza după o mână pierdută/egală
                UpdateBetDisplay();
                PanelBetting.Visibility = Visibility.Visible;
                PanelActions.Visibility = Visibility.Collapsed;

                // Re-enable chips for next round
                foreach (var child in ChipsPanel.Children)
                {
                    if (child is Button b)
                    {
                        b.IsEnabled = true;
                        b.Opacity = 1.0;
                    }
                }
                _selectedChip = null;
            }
        }

        // ================= EFECT DE EXPLOZIE 3D =================
        private void LanseazaArtificii()
        {
            WinOverlay.Visibility = Visibility.Visible;
            WinOverlay.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2)));

            DoubleAnimation textAnim = new DoubleAnimation
            {
                From = 0,
                To = 1.1,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 1.5 }
            };
            TxtWinScale.BeginAnimation(ScaleTransform.ScaleXProperty, textAnim);
            TxtWinScale.BeginAnimation(ScaleTransform.ScaleYProperty, textAnim);

            DoubleAnimation trumpetPulse = new DoubleAnimation { From = 1.0, To = 1.3, Duration = TimeSpan.FromSeconds(0.3), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            TrumpetLeftScale.BeginAnimation(ScaleTransform.ScaleXProperty, trumpetPulse);
            TrumpetLeftScale.BeginAnimation(ScaleTransform.ScaleYProperty, trumpetPulse);

            DoubleAnimation trumpetPulseRight = new DoubleAnimation { From = -1.0, To = -1.3, Duration = TimeSpan.FromSeconds(0.3), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            TrumpetRightScale.BeginAnimation(ScaleTransform.ScaleXProperty, trumpetPulseRight);
            TrumpetRightScale.BeginAnimation(ScaleTransform.ScaleYProperty, trumpetPulse);

            FireworksCanvas.Children.Clear();

            double centerX = WinOverlay.ActualWidth > 0 ? WinOverlay.ActualWidth / 2 : 500;
            double centerY = WinOverlay.ActualHeight > 0 ? WinOverlay.ActualHeight / 2 - 50 : 350;

            Color[] culori = new Color[] { Colors.Gold, Colors.Orange, Colors.Red, Colors.LimeGreen, Colors.Cyan, Colors.White };

            for (int i = 0; i < 120; i++)
            {
                bool isTrail = _random.Next(0, 3) == 0;
                Shape particle;

                if (isTrail)
                {
                    particle = new Rectangle
                    {
                        Width = _random.Next(30, 80),
                        Height = _random.Next(2, 5),
                        Fill = new SolidColorBrush(culori[_random.Next(culori.Length)]),
                        Effect = new DropShadowEffect { Color = Colors.White, BlurRadius = 15, ShadowDepth = 0 }
                    };
                }
                else
                {
                    particle = new Ellipse
                    {
                        Width = _random.Next(8, 20),
                        Height = _random.Next(8, 20),
                        Fill = new SolidColorBrush(culori[_random.Next(culori.Length)]),
                        Effect = new DropShadowEffect { Color = Colors.White, BlurRadius = 15, ShadowDepth = 0 }
                    };
                }

                double angle = _random.NextDouble() * 2 * Math.PI;
                double dist = _random.Next(200, 900);
                double duration = 1.0 + _random.NextDouble() * 1.5;

                TransformGroup tg = new TransformGroup();

                if (isTrail) tg.Children.Add(new RotateTransform(angle * 180 / Math.PI));
                else tg.Children.Add(new RotateTransform(_random.Next(0, 360)));

                ScaleTransform scaleT = new ScaleTransform(0.1, 0.1);
                tg.Children.Add(scaleT);

                TranslateTransform transT = new TranslateTransform(centerX, centerY);
                tg.Children.Add(transT);

                particle.RenderTransformOrigin = new Point(0.5, 0.5);
                particle.RenderTransform = tg;
                FireworksCanvas.Children.Add(particle);

                DoubleAnimation animX = new DoubleAnimation(centerX, centerX + Math.Cos(angle) * dist, TimeSpan.FromSeconds(duration)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                DoubleAnimation animY = new DoubleAnimation(centerY, centerY + Math.Sin(angle) * dist, TimeSpan.FromSeconds(duration)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

                double finalScale = isTrail ? 1.5 : _random.NextDouble() * 2 + 1.5;
                DoubleAnimation animScale = new DoubleAnimation(0.1, finalScale, TimeSpan.FromSeconds(duration)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };

                DoubleAnimation animOpacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(duration)) { BeginTime = TimeSpan.FromSeconds(duration * 0.5) };

                transT.BeginAnimation(TranslateTransform.XProperty, animX);
                transT.BeginAnimation(TranslateTransform.YProperty, animY);
                scaleT.BeginAnimation(ScaleTransform.ScaleXProperty, animScale);
                scaleT.BeginAnimation(ScaleTransform.ScaleYProperty, animScale);
                particle.BeginAnimation(UIElement.OpacityProperty, animOpacity);
            }
        }

        private void BtnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            TrumpetLeftScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            TrumpetLeftScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            TrumpetRightScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            TrumpetRightScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);

            WinOverlay.Visibility = Visibility.Collapsed;
            FireworksCanvas.Children.Clear();

            _currentBet = 0; // Resetăm miza și curățăm ecranul pentru o rundă nouă
            UpdateBetDisplay();

            PanelBetting.Visibility = Visibility.Visible;
            PanelActions.Visibility = Visibility.Collapsed;
            TxtStatus.Text = "Alege miza cu jetoanele și apasă DEAL!";
            TxtStatus.Foreground = Brushes.Gold;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try { BlackJackLobbyWindow lobby = new BlackJackLobbyWindow(_username, _balance.ToString()); lobby.Show(); this.Close(); }
            catch { this.Close(); }
        }
    }
}