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
        private Random _random = new Random();

        // Aici sunt variabilele care îți lipseau (ordinea pe roată și culorile)
        private int[] _wheelOrder = { 0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26 };
        private int[] _redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

        public RouletteGameWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            decimal.TryParse(balance, out _balance);
            UpdateBalanceDisplay();

            // Desenăm roata fix când se deschide fereastra
            DrawWheel();
        }

        private void DrawWheel()
        {
            WheelCanvas.Children.Clear();

            double cx = 175; // Centrul ruletei
            double cy = 175;
            double radius = 175;
            double angleStep = 360.0 / 37.0;

            for (int i = 0; i < 37; i++)
            {
                int number = _wheelOrder[i];

                double startAngle = (i * angleStep) - 90 - (angleStep / 2);
                double endAngle = startAngle + angleStep;
                double midAngle = startAngle + (angleStep / 2);

                System.Windows.Shapes.Path slice = new System.Windows.Shapes.Path();
                slice.Fill = number == 0 ? Brushes.LimeGreen : (_redNumbers.Contains(number) ? Brushes.Red : Brushes.Black);
                slice.Stroke = Brushes.White;
                slice.StrokeThickness = 1;

                PathGeometry geometry = new PathGeometry();
                PathFigure figure = new PathFigure { StartPoint = new Point(cx, cy) };

                Point p1 = new Point(cx + radius * Math.Cos(startAngle * Math.PI / 180), cy + radius * Math.Sin(startAngle * Math.PI / 180));
                Point p2 = new Point(cx + radius * Math.Cos(endAngle * Math.PI / 180), cy + radius * Math.Sin(endAngle * Math.PI / 180));

                figure.Segments.Add(new LineSegment(p1, true));
                figure.Segments.Add(new ArcSegment(p2, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true));
                figure.IsClosed = true;

                geometry.Figures.Add(figure);
                slice.Data = geometry;
                WheelCanvas.Children.Add(slice);

                TextBlock tb = new TextBlock
                {
                    Text = number.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };

                tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                double tw = tb.DesiredSize.Width;
                double th = tb.DesiredSize.Height;

                double textRadius = 145;
                double tx = cx + textRadius * Math.Cos(midAngle * Math.PI / 180);
                double ty = cy + textRadius * Math.Sin(midAngle * Math.PI / 180);

                Canvas.SetLeft(tb, tx - (tw / 2));
                Canvas.SetTop(tb, ty - (th / 2));

                tb.RenderTransform = new RotateTransform(midAngle + 90, tw / 2, th / 2);
                WheelCanvas.Children.Add(tb);
            }

            System.Windows.Shapes.Ellipse innerCircle = new System.Windows.Shapes.Ellipse
            {
                Width = 220,
                Height = 220,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a1a")),
                Stroke = Brushes.Gold,
                StrokeThickness = 3
            };
            Canvas.SetLeft(innerCircle, cx - 110);
            Canvas.SetTop(innerCircle, cy - 110);
            WheelCanvas.Children.Add(innerCircle);
        }

        private void BtnBet_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                _pariuSelectat = btn.Tag.ToString();
                TxtStatus.Text = $"Pariu: {_pariuSelectat} | Pune miza și apasă SPIN!";
                TxtStatus.Foreground = Brushes.Gold;
            }
        }

        private async void BtnSpin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_pariuSelectat))
            {
                MessageBox.Show("Selectează un pariu de pe masă mai întâi!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtBetAmount.Text, out decimal miza) || miza <= 0 || miza > _balance)
            {
                MessageBox.Show("Suma invalidă sau fonduri insuficiente!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _balance -= miza;
            UpdateBalanceDisplay();
            TxtStatus.Text = "Nu se mai acceptă pariuri! Roata se învârte...";
            TxtStatus.Foreground = Brushes.White;

            int indexCastigator = _random.Next(0, 37);
            int numarCastigator = _wheelOrder[indexCastigator];

            double targetAngle = 3600 - (indexCastigator * (360.0 / 37.0));
            DoubleAnimation anim = new DoubleAnimation
            {
                To = targetAngle,
                Duration = TimeSpan.FromSeconds(3),
                DecelerationRatio = 0.8
            };

            WheelRotation.BeginAnimation(RotateTransform.AngleProperty, anim);
            await Task.Delay(3100);

            TxtWheelResult.Text = numarCastigator.ToString();
            VerificaCastig(numarCastigator, miza);
        }

        private void VerificaCastig(int rezultat, decimal miza)
        {
            bool castigat = false;
            decimal multiplicator = 0;

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
                decimal castigTotal = miza * multiplicator;
                _balance += castigTotal;
                TxtStatus.Text = $"CÂȘTIGĂTOR! Numărul a fost {rezultat}. Ai primit {castigTotal} RON.";
                TxtStatus.Foreground = Brushes.LightGreen;
            }
            else
            {
                TxtStatus.Text = $"Ai pierdut. Numărul a fost {rezultat}.";
                TxtStatus.Foreground = Brushes.Tomato;
            }

            UpdateBalanceDisplay();
            _pariuSelectat = "";
        }

        private void UpdateBalanceDisplay() => TxtBalance.Text = $"Sold: {_balance:0.00} RON";

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            RouletteLobbyWindow lobby = new RouletteLobbyWindow(_username, _balance.ToString());
            lobby.Show();
            this.Close();
        }
    }
}