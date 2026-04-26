using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LoseBet.Desktop
{
    public partial class SizzlingHotWindow : Window
    {
        private decimal _balance;
        private string? _username;
        private decimal _currentWin;
        private Random _random = new Random();

        // Numele fișierelor din folderul Assets
        private string[] _symbols = { "7", "star", "watermelon", "grapes", "orange", "lemon", "cherry", "cherry" };

        // Matricea de Border-uri pentru grilă
        private Border[,] _matrix = new Border[3, 5];

        public SizzlingHotWindow(string? username, string? balance)
        {
            InitializeComponent();
            _username = username ?? "Guest";
            decimal.TryParse(balance ?? "0", out _balance);
            UpdateUI();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            SlotsGrid.Children.Clear();

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    var container = new Border
                    {
                        Margin = new Thickness(5),
                        CornerRadius = new CornerRadius(10)
                    };

                    var img = new Image
                    {
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    container.Child = img;
                    SetSymbol(container, img, "cherry");

                    _matrix[r, c] = container;
                    SlotsGrid.Children.Add(container);
                }
            }
        }

        private void SetSymbol(Border container, Image img, string symbolName)
        {
            container.Tag = symbolName;

            try
            {
                string path = $"pack://application:,,,/Assets/{symbolName}.png";
                img.Source = new BitmapImage(new Uri(path, UriKind.Absolute));
                container.Background = Brushes.Transparent;
            }
            catch
            {
                container.Background = Brushes.DarkRed;
                container.Child = new TextBlock
                {
                    Text = $"{symbolName}\n(lipsă poză)",
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold
                };
            }
        }

        private async void BtnSpin_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtBet.Text, out decimal bet) || bet > _balance || bet <= 0) return;

            _balance -= bet;
            UpdateUI();
            BtnSpin.IsEnabled = false;
            PanelDublaj.Visibility = Visibility.Collapsed;

            TxtStatus.Text = "SUCCES!";
            TxtStatus.Foreground = Brushes.White;

            for (int i = 0; i < 15; i++)
            {
                foreach (var container in _matrix)
                {
                    string randomSymbol = _symbols[_random.Next(_symbols.Length)];

                    if (!(container.Child is Image))
                    {
                        container.Child = new Image { Stretch = Stretch.Uniform, Margin = new Thickness(5) };
                    }

                    SetSymbol(container, (Image)container.Child!, randomSymbol);
                }
                await Task.Delay(60);
            }

            CheckWins(bet);
        }

        private void CheckWins(decimal bet)
        {
            decimal win = 0;
            int[][] lines = {
                new int[] {0,0, 0,1, 0,2, 0,3, 0,4},
                new int[] {1,0, 1,1, 1,2, 1,3, 1,4},
                new int[] {2,0, 2,1, 2,2, 2,3, 2,4},
                new int[] {0,0, 1,1, 2,2, 1,3, 0,4},
                new int[] {2,0, 1,1, 0,2, 1,3, 2,4}
            };

            foreach (var line in lines)
            {
                string s1 = _matrix[line[0], line[1]].Tag?.ToString() ?? "";
                int count = 1;

                for (int i = 2; i < 10; i += 2)
                {
                    if (_matrix[line[i], line[i + 1]].Tag?.ToString() == s1) count++;
                    else break;
                }

                if (count >= 3 || (s1 == "cherry" && count >= 2))
                {
                    win += CalculateSymbolWin(s1, count, bet);
                }
            }

            int stars = 0;
            foreach (var container in _matrix)
            {
                if (container.Tag?.ToString() == "star") stars++;
            }
            if (stars >= 3) win += bet * stars * 2;

            if (win > 0)
            {
                _currentWin = win;
                TxtStatus.Text = $"CÂȘTIG: {win} RON! DUBLĂM?";
                TxtStatus.Foreground = Brushes.Gold;
                PanelDublaj.Visibility = Visibility.Visible;
            }
            else
            {
                TxtStatus.Text = "MAI ÎNCEARCĂ!";
                TxtStatus.Foreground = Brushes.LightGray;
                BtnSpin.IsEnabled = true;
            }
        }

        private decimal CalculateSymbolWin(string sym, int count, decimal bet)
        {
            decimal mult = count == 5 ? 100 : (count == 4 ? 20 : 5);
            if (sym == "7") mult *= 10;
            return bet * mult;
        }

        private void BtnGamble_Click(object sender, RoutedEventArgs e)
        {
            if (_random.Next(0, 2) == 0)
            {
                _currentWin *= 2;
                TxtStatus.Text = $"DUBLAT: {_currentWin} RON";
                TxtStatus.Foreground = Brushes.LimeGreen;
            }
            else
            {
                _currentWin = 0;
                PanelDublaj.Visibility = Visibility.Collapsed;
                BtnSpin.IsEnabled = true;
                TxtStatus.Text = "AI PIERDUT!";
                TxtStatus.Foreground = Brushes.Red;
            }
        }

        private void BtnCollect_Click(object sender, RoutedEventArgs e)
        {
            _balance += _currentWin;
            _currentWin = 0;
            UpdateUI();
            PanelDublaj.Visibility = Visibility.Collapsed;
            BtnSpin.IsEnabled = true;
            TxtStatus.Text = "BANI ÎNCASAȚI!";
            TxtStatus.Foreground = Brushes.White;
        }

        private void UpdateUI() => TxtBalance.Text = $"{_balance:0.00}";

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Folosim operatorul ! pentru a ignora avertismentele de null
                new SlotsLobbyWindow(_username!, _balance.ToString()).Show();
                this.Close();
            }
            catch { this.Close(); }
        }
    }
}