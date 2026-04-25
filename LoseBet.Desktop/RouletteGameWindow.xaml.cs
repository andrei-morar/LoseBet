using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LoseBet.Desktop
{
    public partial class RouletteGameWindow : Window
    {
        private decimal _balance;
        private string _username;
        private string _pariuSelectat = "";
        private decimal _sumaDeDublat = 0;
        private Random _random = new Random();

        private double _currentWheelAngle = 0;
        private double _currentBallAngle = 0;

        private int[] _wheelOrder = { 0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26 };
        private int[] _redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

        public RouletteGameWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            decimal.TryParse(balance, out _balance);
            UpdateBalanceDisplay();
            DrawWheel();
        }

        private void DrawWheel()
        {
            WheelCanvas.Children.Clear();
            double cx = 175; double cy = 175; double radius = 175;
            double angleStep = 360.0 / 37.0;

            for (int i = 0; i < 37; i++)
            {
                int number = _wheelOrder[i];
                double startAngle = (i * angleStep) - 90 - (angleStep / 2);
                double endAngle = startAngle + angleStep;
                double midAngle = startAngle + (angleStep / 2);

                Path slice = new Path { Fill = number == 0 ? Brushes.LimeGreen : (_redNumbers.Contains(number) ? Brushes.Red : Brushes.Black), Stroke = Brushes.White, StrokeThickness = 1 };
                PathGeometry geometry = new PathGeometry();
                PathFigure figure = new PathFigure { StartPoint = new Point(cx, cy) };
                figure.Segments.Add(new LineSegment(new Point(cx + radius * Math.Cos(startAngle * Math.PI / 180), cy + radius * Math.Sin(startAngle * Math.PI / 180)), true));
                figure.Segments.Add(new ArcSegment(new Point(cx + radius * Math.Cos(endAngle * Math.PI / 180), cy + radius * Math.Sin(endAngle * Math.PI / 180)), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true));
                figure.IsClosed = true;
                geometry.Figures.Add(figure);
                slice.Data = geometry;
                WheelCanvas.Children.Add(slice);

                TextBlock tb = new TextBlock { Text = number.ToString(), Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.Bold };
                tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                double tw = tb.DesiredSize.Width; double th = tb.DesiredSize.Height;
                double textRadius = 145;
                Canvas.SetLeft(tb, (cx + textRadius * Math.Cos(midAngle * Math.PI / 180)) - (tw / 2));
                Canvas.SetTop(tb, (cy + textRadius * Math.Sin(midAngle * Math.PI / 180)) - (th / 2));
                tb.RenderTransform = new RotateTransform(midAngle + 90, tw / 2, th / 2);
                WheelCanvas.Children.Add(tb);
            }
            Ellipse innerCircle = new Ellipse { Width = 220, Height = 220, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a1a")), Stroke = Brushes.Gold, StrokeThickness = 3 };
            Canvas.SetLeft(innerCircle, cx - 110); Canvas.SetTop(innerCircle, cy - 110);
            WheelCanvas.Children.Add(innerCircle);
        }

        private void BtnBet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                _pariuSelectat = btn.Tag.ToString();
                TxtStatus.Text = $"Pariu: {_pariuSelectat} | Apasă SPIN!";
                TxtStatus.Foreground = Brushes.Gold;
            }
        }

        private async void BtnSpin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_pariuSelectat)) { MessageBox.Show("Selectează un pariu!"); return; }
                if (!decimal.TryParse(TxtBetAmount.Text, out decimal miza) || miza <= 0 || miza > _balance) { MessageBox.Show("Suma invalidă!"); return; }

                _balance -= miza; UpdateBalanceDisplay();
                BtnSpin.IsEnabled = false; TxtStatus.Text = "Norocul se învârte..."; TxtWheelResult.Opacity = 0;

                int indexCastigator = _random.Next(0, 37);
                int numarCastigator = _wheelOrder[indexCastigator];

                // --- LOGICA DE ROTAȚIE RANDOMIZATĂ ---
                double anglePerSlot = 360.0 / 37.0;
                double slotAngle = indexCastigator * anglePerSlot;

                // Alegem un punct de oprire vizuală pentru BILĂ complet random pe cerc
                double randomVisualOffset = _random.NextDouble() * 360.0;

                // Bila se va roti 8 ture + offset-ul random
                double ballTargetAngle = _currentBallAngle + (360 * 8) + randomVisualOffset;

                // Roata trebuie să se oprească astfel încât buzunarul numărului să fie sub bilă
                // Formula: UnghiulBilei - UnghiulSlotuluiPeRoată
                double wheelTargetAngle = ballTargetAngle - slotAngle;

                // Animație ROATĂ
                DoubleAnimation wheelAnim = new DoubleAnimation { To = wheelTargetAngle, Duration = TimeSpan.FromSeconds(2.8), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                // Animație BILĂ
                DoubleAnimation ballAnim = new DoubleAnimation { To = ballTargetAngle, Duration = TimeSpan.FromSeconds(3.8), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

                WheelRotation.BeginAnimation(RotateTransform.AngleProperty, wheelAnim);
                BallRotation.BeginAnimation(RotateTransform.AngleProperty, ballAnim);

                _currentWheelAngle = wheelTargetAngle;
                _currentBallAngle = ballTargetAngle;

                await Task.Delay(4000);

                Application.Current.Dispatcher.Invoke(() => {
                    TxtWheelResult.Opacity = 1; TxtWheelResult.Text = numarCastigator.ToString();
                    TxtWheelResult.Foreground = numarCastigator == 0 ? Brushes.LimeGreen : (_redNumbers.Contains(numarCastigator) ? Brushes.Tomato : Brushes.White);
                    VerificaCastig(numarCastigator, miza);
                });
            }
            catch (Exception ex) { MessageBox.Show("Eroare: " + ex.Message); BtnSpin.IsEnabled = true; }
        }

        private void VerificaCastig(int rezultat, decimal miza)
        {
            bool castigat = false; decimal multiplicator = 0;
            if (_pariuSelectat == rezultat.ToString()) { castigat = true; multiplicator = 36; }
            else if (_pariuSelectat == "Red" && _redNumbers.Contains(rezultat)) { castigat = true; multiplicator = 2; }
            else if (_pariuSelectat == "Black" && rezultat != 0 && !_redNumbers.Contains(rezultat)) { castigat = true; multiplicator = 2; }
            else if (_pariuSelectat == "Even" && rezultat != 0 && rezultat % 2 == 0) { castigat = true; multiplicator = 2; }
            else if (_pariuSelectat == "Odd" && rezultat % 2 != 0) { castigat = true; multiplicator = 2; }
            else if (_pariuSelectat == "1to18" && rezultat >= 1 && rezultat <= 18) { castigat = true; multiplicator = 2; }
            else if (_pariuSelectat == "19to36" && rezultat >= 19 && rezultat <= 36) { castigat = true; multiplicator = 2; }
            else if (_pariuSelectat == "1st12" && rezultat >= 1 && rezultat <= 12) { castigat = true; multiplicator = 3; }
            else if (_pariuSelectat == "2nd12" && rezultat >= 13 && rezultat <= 24) { castigat = true; multiplicator = 3; }
            else if (_pariuSelectat == "3rd12" && rezultat >= 25 && rezultat <= 36) { castigat = true; multiplicator = 3; }

            if (castigat)
            {
                _sumaDeDublat = miza * multiplicator;
                TxtStatus.Text = $"CÂȘTIG! {rezultat} a fost norocos. Dublezi {_sumaDeDublat} RON?";
                TxtStatus.Foreground = Brushes.LightGreen;
                ArataEcranVictorie();
            }
            else
            {
                TxtStatus.Text = $"Ai pierdut. Numărul a fost {rezultat}.";
                TxtStatus.Foreground = Brushes.Tomato;
                _pariuSelectat = ""; BtnSpin.IsEnabled = true;
            }
        }

        private void ArataEcranVictorie()
        {
            VictoryOverlay.Visibility = Visibility.Visible; VictoryOverlay.IsHitTestVisible = true;
            VictoryOverlay.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { To = 1, Duration = TimeSpan.FromSeconds(0.5) });
            DoubleAnimation bounce = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(1), EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut } };
            WinTextScale.BeginAnimation(ScaleTransform.ScaleXProperty, bounce); WinTextScale.BeginAnimation(ScaleTransform.ScaleYProperty, bounce);
        }

        private async void ProceseazaDublajul()
        {
            if (_random.Next(0, 2) == 1)
            {
                _sumaDeDublat *= 2; TxtDoubleMsg.Text = "⭐ AI DUBLAT! ⭐"; TxtDoubleMsg.Foreground = Brushes.LimeGreen;
                DoubleSuccessOverlay.Visibility = Visibility.Visible; DoubleSuccessOverlay.IsHitTestVisible = true;
                DoubleSuccessOverlay.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { To = 1, Duration = TimeSpan.FromSeconds(0.3) });
                VictoryOverlay.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { To = 0, Duration = TimeSpan.FromSeconds(0.3) });
                await Task.Delay(300); VictoryOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                _sumaDeDublat = 0; TxtStatus.Text = "Ai pierdut tot la dublaj!"; TxtStatus.Foreground = Brushes.Red;
                AscundeToateOverlayurile();
            }
        }

        private void BtnDoubleRed_Click(object sender, RoutedEventArgs e) => ProceseazaDublajul();
        private void BtnDoubleBlack_Click(object sender, RoutedEventArgs e) => ProceseazaDublajul();
        private void BtnCollect_Click(object sender, RoutedEventArgs e) { _balance += _sumaDeDublat; UpdateBalanceDisplay(); AscundeToateOverlayurile(); }
        private void BtnCloseDouble_Click(object sender, RoutedEventArgs e) { _balance += _sumaDeDublat; UpdateBalanceDisplay(); AscundeToateOverlayurile(); }

        private void AscundeToateOverlayurile()
        {
            VictoryOverlay.BeginAnimation(UIElement.OpacityProperty, null); DoubleSuccessOverlay.BeginAnimation(UIElement.OpacityProperty, null);
            VictoryOverlay.Visibility = Visibility.Collapsed; DoubleSuccessOverlay.Visibility = Visibility.Collapsed;
            VictoryOverlay.IsHitTestVisible = false; DoubleSuccessOverlay.IsHitTestVisible = false;
            BtnSpin.IsEnabled = true; _pariuSelectat = "";
        }

        private void UpdateBalanceDisplay() => TxtBalance.Text = $"Sold: {_balance:0.00} RON";
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try { RouletteLobbyWindow lobby = new RouletteLobbyWindow(_username, _balance.ToString()); lobby.Show(); this.Close(); }
            catch { this.Close(); }
        }
    }
}