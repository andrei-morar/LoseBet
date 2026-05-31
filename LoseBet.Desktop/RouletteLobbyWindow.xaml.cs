using System.Windows;

namespace LoseBet.Desktop
{
    public partial class RouletteLobbyWindow : Window
    {
        private string _username;
        private string _balance;

        public RouletteLobbyWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            _balance = balance;
            TxtBalance.Text = $"Sold: {_balance} RON";
        }

        private void BtnPlayEuro_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            RouletteGameWindow game = new RouletteGameWindow(_username, rawBalance);
            game.Show();
            this.Close();
        }

        // Am șters funcția BtnPlayAmer_Click de aici.

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_username, _balance);
            dashboard.Show();
            this.Close();
        }
    }
}