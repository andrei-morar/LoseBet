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
        private decimal _currentBet;

        private List<string> _deck = new List<string>();
        private List<string> _playerCards = new List<string>();
        private List<string> _dealerCards = new List<string>();
        private Random _random = new Random();

        public BlackjackGameWindow(string? username, string? balance)
        {
            InitializeComponent();
            _username = username ?? "Guest";
            decimal.TryParse(balance ?? "0", out _balance);
            UpdateBalanceDisplay();
        }

        private void UpdateBalanceDisplay() => TxtBalance.Text = $"Sold: {_balance:0.00} RON";

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
                // SCHIMBARE AICI: Albastru închis pentru spatele cărții (se potrivește cu masa Classic)
                b.Background = new SolidColorBrush(Color.FromRgb(0, 34, 102)); // Același albastru #002266
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
            if (!decimal.TryParse(TxtBetAmount.Text, out _currentBet) || _currentBet <= 0 || _currentBet > _balance)
            {
                MessageBox.Show("Miza invalidă sau fonduri insuficiente!"); return;
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
                LanseazaArtificii(); // Lansăm explozia magică
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

                PanelBetting.Visibility = Visibility.Visible;
                PanelActions.Visibility = Visibility.Collapsed;
            }
        }

        // ================= EFECT DE EXPLOZIE 3D (ZBOARĂ SPRE TINE) =================
        private void LanseazaArtificii()
        {
            WinOverlay.Visibility = Visibility.Visible;
            WinOverlay.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2)));

            // 1. Textul "Sare" agresiv spre tine
            DoubleAnimation textAnim = new DoubleAnimation
            {
                From = 0,
                To = 1.1,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 1.5 }
            };
            TxtWinScale.BeginAnimation(ScaleTransform.ScaleXProperty, textAnim);
            TxtWinScale.BeginAnimation(ScaleTransform.ScaleYProperty, textAnim);

            // 2. Pulsarea Trompetelor
            DoubleAnimation trumpetPulse = new DoubleAnimation
            {
                From = 1.0,
                To = 1.3,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            TrumpetLeftScale.BeginAnimation(ScaleTransform.ScaleXProperty, trumpetPulse);
            TrumpetLeftScale.BeginAnimation(ScaleTransform.ScaleYProperty, trumpetPulse);

            DoubleAnimation trumpetPulseRight = new DoubleAnimation
            {
                From = -1.0,
                To = -1.3,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            TrumpetRightScale.BeginAnimation(ScaleTransform.ScaleXProperty, trumpetPulseRight);
            TrumpetRightScale.BeginAnimation(ScaleTransform.ScaleYProperty, trumpetPulse);

            // 3. ARTIFICIILE EXPLODEAZĂ DIN CENTRU SPRE ECRAN (Efect 3D)
            FireworksCanvas.Children.Clear();

            // Centrul exploziei (fix sub text)
            double centerX = WinOverlay.ActualWidth > 0 ? WinOverlay.ActualWidth / 2 : 500;
            double centerY = WinOverlay.ActualHeight > 0 ? WinOverlay.ActualHeight / 2 - 50 : 350;

            Color[] culori = new Color[] { Colors.Gold, Colors.Orange, Colors.Red, Colors.LimeGreen, Colors.Cyan, Colors.White };

            for (int i = 0; i < 120; i++) // 120 de particule pentru un impact major
            {
                bool isTrail = _random.Next(0, 3) == 0; // O parte din ele vor fi dâre lungi de lumină
                Shape particle;

                if (isTrail)
                {
                    // Dâră de lumină
                    particle = new Rectangle
                    {
                        Width = _random.Next(30, 80),
                        Height = _random.Next(2, 5),
                        Fill = new SolidColorBrush(culori[_random.Next(culori.Length)]),
                        Effect = new DropShadowEffect { Color = Colors.White, BlurRadius = 15, ShadowDepth = 0 } // Glow puternic
                    };
                }
                else
                {
                    // Stele / Puncte
                    particle = new Ellipse
                    {
                        Width = _random.Next(8, 20),
                        Height = _random.Next(8, 20),
                        Fill = new SolidColorBrush(culori[_random.Next(culori.Length)]),
                        Effect = new DropShadowEffect { Color = Colors.White, BlurRadius = 15, ShadowDepth = 0 }
                    };
                }

                // Generăm mișcarea
                double angle = _random.NextDouble() * 2 * Math.PI;
                double dist = _random.Next(200, 900); // Se duc mult în afara ecranului
                double duration = 1.0 + _random.NextDouble() * 1.5; // Între 1 și 2.5 secunde

                // Pregătim elementele pentru transformări
                TransformGroup tg = new TransformGroup();

                // Rotim dârele de lumină astfel încât să urmeze direcția în care zboară
                if (isTrail) tg.Children.Add(new RotateTransform(angle * 180 / Math.PI));
                else tg.Children.Add(new RotateTransform(_random.Next(0, 360)));

                ScaleTransform scaleT = new ScaleTransform(0.1, 0.1); // Pleacă microscopice (din spatele textului)
                tg.Children.Add(scaleT);

                TranslateTransform transT = new TranslateTransform(centerX, centerY);
                tg.Children.Add(transT);

                particle.RenderTransformOrigin = new Point(0.5, 0.5);
                particle.RenderTransform = tg;
                FireworksCanvas.Children.Add(particle);

                // Animație 1: Deplasare
                DoubleAnimation animX = new DoubleAnimation(centerX, centerX + Math.Cos(angle) * dist, TimeSpan.FromSeconds(duration)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                DoubleAnimation animY = new DoubleAnimation(centerY, centerY + Math.Sin(angle) * dist, TimeSpan.FromSeconds(duration)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

                // Animație 2: Mărire (creează iluzia că zboară SPRE TINE)
                double finalScale = isTrail ? 1.5 : _random.NextDouble() * 2 + 1.5; // Se fac mari de 2-3 ori
                DoubleAnimation animScale = new DoubleAnimation(0.1, finalScale, TimeSpan.FromSeconds(duration)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };

                // Animație 3: Fade Out la final
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

            PanelBetting.Visibility = Visibility.Visible;
            PanelActions.Visibility = Visibility.Collapsed;
            TxtStatus.Text = "Pune miza și apasă DEAL!";
            TxtStatus.Foreground = Brushes.Gold;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try { BlackJackLobbyWindow lobby = new BlackJackLobbyWindow(_username, _balance.ToString()); lobby.Show(); this.Close(); }
            catch { this.Close(); }
        }
    }
}