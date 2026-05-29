using LoseBet.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace LoseBet.Desktop
{
    public partial class AviatorWindow : Window
    {
        private DispatcherTimer _gameTimer;
        private DispatcherTimer _waitTimer;

        private decimal _currentMultiplier = 1.00m;
        private decimal _crashPoint;
        private int _currentSessionId;

        private enum GameState { Waiting, Flying, Crashed }
        private GameState _currentState = GameState.Crashed;

        private bool _hasBetForNextRound = false;
        private decimal _stagedBetAmount = 0;
        private decimal _currentBalance = 0;

        // Auto cashout
        private bool _activeAutoCashout = false;
        private decimal _activeTargetMultiplier = 0;

        // Animatie zbor
        private double _timeElapsed = 0;
        private double _prevX = 0;
        private double _prevY = 0;

        // Lăcatul pentru închidere
        private bool _isClosing = false;

        // Istoric
        private ObservableCollection<HistoryItem> _history = new ObservableCollection<HistoryItem>();

        public AviatorWindow()
        {
            InitializeComponent();

            HistoryPanel.ItemsSource = _history;

            _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _gameTimer.Tick += GameTimer_Tick;

            _waitTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(60) };
            _waitTimer.Tick += WaitTimer_Tick;

            RefreshBalance();
            AddFakeHistory();
            StartWaitPhase();
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
                // Eroare retea silențioasă pe background
            }
        }

        // ==========================================
        // CICLUL JOCULUI
        // ==========================================
        private void StartWaitPhase()
        {
            _currentState = GameState.Waiting;

            MultiplierDisplay.Text = "1.00x";
            MultiplierDisplay.Foreground = Brushes.White;
            MultiplierDisplay.FontSize = 90;
            SetMultiplierGlow("#E53935", 0.4);

            StatusText.Text = "A Ș T E P T A R E   P A R I U R I";
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
            SetStatusGlow("#FF9800");

            WaitProgressBar.Visibility = Visibility.Visible;
            WaitProgressBar.Value = 100;
            ResetCanvas();

            if (_hasBetForNextRound) SetBtnStateCashoutReady();
            else SetBtnStateBet();

            _waitTimer.Start();
        }

        private void WaitTimer_Tick(object? sender, EventArgs e)
        {
            WaitProgressBar.Value -= 1.8;
            if (WaitProgressBar.Value <= 0)
            {
                _waitTimer.Stop();
                StartFlightPhase();
            }
        }

        private void StartFlightPhase()
        {
            _currentState = GameState.Flying;

            WaitProgressBar.Visibility = Visibility.Hidden;
            StatusText.Text = "";

            if (!_hasBetForNextRound)
                _crashPoint = GenerateLocalCrashPoint();

            _currentMultiplier = 1.00m;
            _timeElapsed = 0;
            _prevX = _prevY = 0;

            PlaneIcon.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
            PlaneIcon.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            MultiplierDisplay.Foreground = Brushes.White;
            SetMultiplierGlow("#E53935", 0.5);

            if (_hasBetForNextRound) SetBtnStateCashout();
            else SetBtnStateWaitingNext();

            _gameTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            _currentMultiplier += 0.003m + (_currentMultiplier * 0.007m);
            _timeElapsed += 0.03;

            AnimatePlane(_timeElapsed, (double)_currentMultiplier);
            UpdateMultiplierColor();

            if (_hasBetForNextRound && _currentState == GameState.Flying && _activeAutoCashout)
            {
                if (_currentMultiplier >= _activeTargetMultiplier)
                    _ = ExecuteCashout(_activeTargetMultiplier);
            }

            if (_currentMultiplier >= _crashPoint)
            {
                Crash();
                return;
            }

            MultiplierDisplay.Text = $"{_currentMultiplier:F2}x";

            if (_hasBetForNextRound && _currentState == GameState.Flying)
                BtnBetSubText.Text = $"Câștig: {(_stagedBetAmount * _currentMultiplier):F2} RON";
        }

        private void Crash()
        {
            _gameTimer.Stop();
            _currentState = GameState.Crashed;
            _hasBetForNextRound = false;

            AnimateCrash();

            MultiplierDisplay.Text = $"💥  {_crashPoint:F2}x";
            MultiplierDisplay.Foreground = new SolidColorBrush(Colors.Red);
            SetMultiplierGlow("#FF0000", 0.9);

            StatusText.Text = "F L E W   A W A Y !";
            StatusText.Foreground = Brushes.Red;
            SetStatusGlow("#FF0000");

            AddToHistory(_crashPoint);
            SetBtnStateBet();

            Task.Delay(3200).ContinueWith(t => Dispatcher.Invoke(() =>
            {
                if (!_isClosing) StartWaitPhase();
            }));
        }

        // ==========================================
        // PARIERE SI CASHOUT
        // ==========================================
        private async void BtnBet_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == GameState.Waiting && !_hasBetForNextRound)
            {
                if (!decimal.TryParse(BetInput.Text, out decimal betAmount) || betAmount <= 0)
                {
                    FlashStatus("MIZĂ INVALIDĂ!", "#FF5252");
                    return;
                }

                var result = await GameService.StartAviatorAsync(betAmount);
                if (result != null)
                {
                    _currentSessionId = result.SessionId;
                    _crashPoint = result.CrashPoint;
                    _stagedBetAmount = betAmount;
                    _hasBetForNextRound = true;

                    if (ChkAutoCashout.IsChecked == true &&
                        decimal.TryParse(AutoCashoutInput.Text, out decimal target) && target > 1.00m)
                    {
                        _activeAutoCashout = true;
                        _activeTargetMultiplier = target;
                    }
                    else
                    {
                        _activeAutoCashout = false;
                    }

                    RefreshBalance();
                    SetBtnStateCashoutReady();
                }
                else
                {
                    FlashStatus("FONDURI INSUFICIENTE SAU EROARE!", "#FF5252");
                }
            }
            else if (_currentState == GameState.Flying && _hasBetForNextRound)
            {
                await ExecuteCashout();
            }
        }

        private async Task ExecuteCashout(decimal? overrideMultiplier = null)
        {
            BtnBet.IsEnabled = false;

            decimal mult = overrideMultiplier ?? Math.Round(_currentMultiplier, 2);
            decimal profit = _stagedBetAmount * mult;

            var result = await GameService.CashoutAviatorAsync(_currentSessionId, mult);

            if (result != null && result.Success)
            {
                _hasBetForNextRound = false;

                ShowCashoutBanner($"💰  CASHOUT  {mult:F2}x  →  +{profit:F2} RON");

                SetBtnStateWaitingNext();
                RefreshBalance();
            }
            else
            {
                BtnBet.IsEnabled = true;
            }
        }

        // ==========================================
        // STĂRI BUTON
        // ==========================================
        private void SetBtnStateBet()
        {
            SetBtnGradient("#1E7A34", "#28A745");
            SetBtnGlowColor("#00E676");
            BtnBetMainText.Text = "PARIAZĂ";
            BtnBetSubText.Visibility = Visibility.Collapsed;
            BtnBet.IsEnabled = true;
            BetInput.IsEnabled = true;
            ToggleQuickBtns(true);
            if (ChkAutoCashout != null) ChkAutoCashout.IsEnabled = true;
            if (AutoCashoutInput != null) AutoCashoutInput.IsEnabled = true;
        }

        private void SetBtnStateCashoutReady()
        {
            SetBtnGradient("#7A1A1A", "#A52020");
            SetBtnGlowColor("#FF3333");
            BtnBetMainText.Text = "AȘTEAPTĂ ZBORUL";
            BtnBetSubText.Visibility = Visibility.Collapsed;
            BtnBet.IsEnabled = false;
            BetInput.IsEnabled = false;
            ToggleQuickBtns(false);
            if (ChkAutoCashout != null) ChkAutoCashout.IsEnabled = false;
            if (AutoCashoutInput != null) AutoCashoutInput.IsEnabled = false;
        }

        private void SetBtnStateCashout()
        {
            SetBtnGradient("#E65100", "#FF9800");
            SetBtnGlowColor("#FF9800");
            BtnBetMainText.Text = "CASHOUT";
            BtnBetSubText.Text = $"Câștig: {_stagedBetAmount:F2} RON";
            BtnBetSubText.Visibility = Visibility.Visible;
            BtnBet.IsEnabled = true;
        }

        private void SetBtnStateWaitingNext()
        {
            SetBtnGradient("#222230", "#333345");
            SetBtnGlowColor("#555566");
            BtnBetMainText.Text = "AȘTEAPTĂ RUNDA VIITOARE";
            BtnBetSubText.Visibility = Visibility.Collapsed;
            BtnBet.IsEnabled = false;
        }

        // ==========================================
        // ANIMAȚIE CANVAS 
        // ==========================================
        private void ResetCanvas()
        {
            FlightPath.Data = null;
            FlightFill.Data = null;
            PlaneBrush.Color = (Color)ColorConverter.ConvertFromString("#E53935");
            PlaneIcon.Opacity = 1;
            PlaneRotation.Angle = -10;

            double h = FlightCanvas.ActualHeight > 0 ? FlightCanvas.ActualHeight : 350;
            Canvas.SetLeft(PlaneIcon, 20);
            Canvas.SetTop(PlaneIcon, h - 70);
        }

        private void AnimatePlane(double time, double multiplier)
        {
            double w = FlightCanvas.ActualWidth > 0 ? FlightCanvas.ActualWidth : 800;
            double h = FlightCanvas.ActualHeight > 0 ? FlightCanvas.ActualHeight : 350;

            double x = Math.Min(time * 48, w - 80);

            double curveFactor = Math.Min((multiplier - 1.0) / 8.0, 1.0);
            double ease = Math.Pow(curveFactor, 0.65);
            double y = (h - 55) - ((h - 80) * ease);

            double dx = x - _prevX;
            double dy = y - _prevY;
            double angle = (dx > 0) ? Math.Atan2(-dy, dx) * (180.0 / Math.PI) : -10;
            angle = Math.Max(-40, Math.Min(5, -angle));

            PlaneRotation.Angle = angle;
            Canvas.SetLeft(PlaneIcon, x - 20);
            Canvas.SetTop(PlaneIcon, y - 24);

            double ctrlX = x * 0.5;
            double ctrlY = h - 55;
            string pathStr = $"M 0,{h - 55:F1} Q {ctrlX:F1},{ctrlY:F1} {x:F1},{y:F1}";
            try
            {
                FlightPath.Data = Geometry.Parse(pathStr);
                string fillStr = $"M 0,{h - 55:F1} Q {ctrlX:F1},{ctrlY:F1} {x:F1},{y:F1} L {x:F1},{h - 55:F1} Z";
                FlightFill.Data = Geometry.Parse(fillStr);
            }
            catch { }

            _prevX = x;
            _prevY = y;
        }

        private void AnimateCrash()
        {
            var rotAnim = new DoubleAnimation(-10, 90, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            PlaneRotation.BeginAnimation(RotateTransform.AngleProperty, rotAnim);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(800))
            {
                BeginTime = TimeSpan.FromMilliseconds(300)
            };
            PlaneIcon.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            FlightPath.Stroke = new SolidColorBrush(Color.FromArgb(120, 100, 100, 100));
            FlightFill.Fill = new SolidColorBrush(Color.FromArgb(15, 100, 100, 100));
        }

        private async void ShowCashoutBanner(string text)
        {
            if (_isClosing) return;

            CashoutBannerText.Text = text;
            CashoutBanner.Visibility = Visibility.Visible;
            CashoutBanner.Opacity = 0;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400))
            {
                BeginTime = TimeSpan.FromMilliseconds(2200)
            };
            fadeOut.Completed += (s, e) => CashoutBanner.Visibility = Visibility.Collapsed;

            CashoutBanner.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            await Task.Delay(2200);
            CashoutBanner.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void UpdateMultiplierColor()
        {
            if (_currentMultiplier < 2m)
            {
                MultiplierDisplay.Foreground = Brushes.White;
                SetMultiplierGlow("#E53935", 0.4);
            }
            else if (_currentMultiplier < 5m)
            {
                MultiplierDisplay.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
                SetMultiplierGlow("#FFD700", 0.6);
            }
            else if (_currentMultiplier < 10m)
            {
                MultiplierDisplay.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                SetMultiplierGlow("#FF9800", 0.7);
            }
            else
            {
                MultiplierDisplay.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676"));
                SetMultiplierGlow("#00E676", 0.9);
                MultiplierDisplay.FontSize = Math.Min(110 + (double)(_currentMultiplier - 10) * 1.5, 130);
            }
        }

        private void SetMultiplierGlow(string hexColor, double opacity)
        {
            if (MultiplierDisplay.Effect is DropShadowEffect glow)
            {
                glow.Color = (Color)ColorConverter.ConvertFromString(hexColor);
                glow.Opacity = opacity;
            }
        }

        private void SetStatusGlow(string hexColor)
        {
            if (StatusText.Effect is DropShadowEffect g)
                g.Color = (Color)ColorConverter.ConvertFromString(hexColor);
        }

        private void SetBtnGradient(string startHex, string endHex)
        {
            BtnGradStart.Color = (Color)ColorConverter.ConvertFromString(startHex);
            BtnGradEnd.Color = (Color)ColorConverter.ConvertFromString(endHex);
        }

        private void SetBtnGlowColor(string hexColor)
        {
            BtnGlow.Color = (Color)ColorConverter.ConvertFromString(hexColor);
        }

        private void FlashStatus(string text, string colorHex)
        {
            StatusText.Text = text;
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
        }

        private void ToggleQuickBtns(bool enabled)
        {
            QBtn1.IsEnabled = QBtn2.IsEnabled = QBtn3.IsEnabled = QBtn4.IsEnabled = enabled;
        }

        private void QuickBet_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == GameState.Flying || _hasBetForNextRound) return;
            var tag = (sender as Button)?.Tag?.ToString();
            if (tag == null) return;

            if (tag == "max")
            {
                BetInput.Text = _currentBalance.ToString("F2");
                return;
            }
            if (decimal.TryParse(BetInput.Text, out decimal cur) &&
                decimal.TryParse(tag, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal factor))
            {
                decimal nv = Math.Round(cur * factor, 2);
                BetInput.Text = (nv < 1 ? 1 : nv).ToString("F2");
            }
        }

        private void BtnLowerBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BetInput.Text, out decimal val) && val > 1)
                BetInput.Text = (val - 1).ToString("F2");
        }

        private void BtnHigherBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BetInput.Text, out decimal val))
                BetInput.Text = (val + 1).ToString("F2");
        }

        private void AddToHistory(decimal multiplier)
        {
            if (_history.Count >= 12) _history.RemoveAt(_history.Count - 1);

            string color;
            if (multiplier < 1.5m) color = "#1A1A24";
            else if (multiplier < 2m) color = "#2E303E";
            else if (multiplier < 5m) color = "#5E35B1";
            else if (multiplier < 10m) color = "#D81B60";
            else color = "#00E676";

            _history.Insert(0, new HistoryItem { Value = multiplier, ColorCode = color });
        }

        private void AddFakeHistory()
        {
            AddToHistory(1.12m); AddToHistory(4.50m); AddToHistory(1.88m);
            AddToHistory(12.30m); AddToHistory(1.05m); AddToHistory(2.75m); AddToHistory(8.40m);
        }

        private decimal GenerateLocalCrashPoint()
        {
            Random rnd = new Random();
            double val = 100.0 / (rnd.NextDouble() * 100 + 1);
            decimal crash = Math.Round((decimal)val, 2);
            return crash < 1.01m ? 1.01m : crash;
        }

        // AICI ESTE LĂCATUL APLICAT PENTRU AVIATOR
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_isClosing) return;
            _isClosing = true;

            if (sender is Button btn) btn.IsEnabled = false;

            _gameTimer?.Stop();
            _waitTimer?.Stop();

            DashboardWindow dashboard = new DashboardWindow("", "");
            dashboard.Show();
            this.Close();
        }
    }

    public class HistoryItem
    {
        public decimal Value { get; set; }
        public string? ColorCode { get; set; }
    }
}