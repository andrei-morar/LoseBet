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
            decimal.TryParse(balance, out _balance);
            TxtBalance.Text = $"Sold: {_balance:0.00} RON";
        }

        private void BtnStartGame_Click(object sender, RoutedEventArgs e)
        {
            // Extragem miza selectată
            string selectedBetStr = (CmbBetAmount.SelectedItem as ComboBoxItem).Content.ToString();
            decimal betAmount = decimal.Parse(selectedBetStr);

            if (betAmount > _balance)
            {
                MessageBox.Show("Fonduri insuficiente pentru această miză!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Extragem numărul de boți
            int botCount = 1;
            if (Rb2Bots.IsChecked == true) botCount = 2;
            else if (Rb5Bots.IsChecked == true) botCount = 5;

            // Verificăm să fie măcar o categorie bifată
            if (ChkSport.IsChecked == false && ChkGeo.IsChecked == false && ChkIstorie.IsChecked == false &&
                ChkArta.IsChecked == false && ChkPoeti.IsChecked == false && ChkStiinta.IsChecked == false && ChkDiverse.IsChecked == false)
            {
                MessageBox.Show("Te rog să alegi cel puțin o categorie de întrebări!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Scădem banii pentru intrarea în meci
            _balance -= betAmount;

            // LANSĂM JOCUL PROPRIU-ZIS ȘI ÎI TRIMITEM TOATE DATELE!
            TriviadorGameWindow gameWindow = new TriviadorGameWindow(_username, _balance, betAmount, botCount);
            gameWindow.Show();
            this.Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_username, _balance.ToString());
            dashboard.Show();
            this.Close();
        }
    }
}