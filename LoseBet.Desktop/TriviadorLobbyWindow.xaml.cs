using System;
using System.Windows;
using System.Windows.Controls;

namespace LoseBet.Desktop
{
    public partial class TriviadorLobbyWindow : Window
    {
        private string _username;
        private decimal _balance;

        public TriviadorLobbyWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;

            if (decimal.TryParse(balance, out decimal parsedBalance))
            {
                _balance = parsedBalance;
            }

            TxtBalance.Text = $"{_balance:0.00} RON";
        }

        // ==========================================
        // BUTOANE QUICK BET SI ADJUSTARI MIZA
        // ==========================================
        private void QuickBet_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag?.ToString();
            if (tag == null) return;

            if (tag == "max")
            {
                TxtBet.Text = _balance.ToString("F2");
                return;
            }

            if (decimal.TryParse(TxtBet.Text, out decimal cur) &&
                decimal.TryParse(tag, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal factor))
            {
                decimal nv = Math.Round(cur * factor, 2);
                TxtBet.Text = (nv < 1 ? 1 : nv).ToString("F2");
            }
        }

        private void BtnLowerBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtBet.Text, out decimal val) && val > 1)
            {
                TxtBet.Text = (val - 1).ToString("F2");
            }
        }

        private void BtnHigherBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtBet.Text, out decimal val))
            {
                TxtBet.Text = (val + 1).ToString("F2");
            }
        }

        // ==========================================
        // LANSĂM JOCUL
        // ==========================================
        private void BtnStartGame_Click(object sender, RoutedEventArgs e)
        {
            // Extragem miza din noul TextBox
            if (!decimal.TryParse(TxtBet.Text, out decimal betAmount) || betAmount <= 0)
            {
                MessageBox.Show("Introdu o miză validă!", "Eroare Miza", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (betAmount > _balance)
            {
                MessageBox.Show("Fonduri insuficiente pentru această miză!", "Eroare Fonduri", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Extragem numărul de boți din noile RadioButtons
            int botCount = 1;
            if (Rb2Bots.IsChecked == true) botCount = 2;
            else if (Rb5Bots.IsChecked == true) botCount = 5;

            // Verificăm să fie măcar o categorie bifată
            if (ChkSport.IsChecked == false && ChkGeo.IsChecked == false && ChkIstorie.IsChecked == false &&
                ChkArta.IsChecked == false && ChkPoeti.IsChecked == false && ChkStiinta.IsChecked == false && ChkDiverse.IsChecked == false)
            {
                MessageBox.Show("Te rog să alegi cel puțin o categorie de întrebări!", "Atenție Categorii", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Scădem banii pentru intrarea în meci
            _balance -= betAmount;

            // LANSĂM JOCUL PROPRIU-ZIS ȘI ÎI TRIMITEM TOATE DATELE
            TriviadorGameWindow gameWindow = new TriviadorGameWindow(_username, _balance, betAmount, botCount);
            gameWindow.Show();
            this.Close();
        }

        // ==========================================
        // INAPOI LA DASHBOARD
        // ==========================================
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_username, _balance.ToString("F2"));
            dashboard.Show();
            this.Close();
        }
    }
}