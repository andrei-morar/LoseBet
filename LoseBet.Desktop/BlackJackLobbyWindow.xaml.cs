using System.Windows;

namespace LoseBet.Desktop
{
    // Aici am pus 'J' mare ca să se potrivească perfect cu design-ul din XAML!
    public partial class BlackJackLobbyWindow : Window
    {
        private string _username;
        private string _balance;

        public BlackJackLobbyWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            _balance = balance;
            TxtBalance.Text = $"Sold: {_balance} RON";
        }

        private void BtnPlayClassic_Click(object sender, RoutedEventArgs e)
        {
            // Deschidem fereastra cu masa de joc (pe care o creăm la Pasul 2)
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            BlackjackGameWindow game = new BlackjackGameWindow(_username, rawBalance);
            game.Show();
            this.Close();
        }

        private void BtnPlayVIP_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Masa VIP este rezervată momentan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_username, _balance);
            dashboard.Show();
            this.Close();
        }
    }
}