using System.Windows;

namespace LoseBet.Desktop
{
    public partial class SlotsLobbyWindow : Window
    {
        private string _username;
        private string _balance;

        public SlotsLobbyWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            _balance = balance;

            // Afișăm soldul primit din pagina anterioară
            TxtBalance.Text = $"Sold: {_balance} RON";
        }

        private void BtnPlayFruit_Click(object sender, RoutedEventArgs e)
        {
            // Asta te duce în jocul 3x5 pe care l-am construit deja
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            SlotsWindow gameWindow = new SlotsWindow(_username, rawBalance);
            gameWindow.Show();
            this.Close();
        }

        // --- BUTOANELE NOI ---

        private void BtnPlaySizzling_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            SizzlingHotWindow sizzling = new SizzlingHotWindow(_username, rawBalance);
            sizzling.Show();
            this.Close();
        }

        private void BtnPlayBurning_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Burning Hot se încarcă în agenție... În curând!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Ne întoarcem la Dashboard
            DashboardWindow dashboard = new DashboardWindow(_username, _balance);
            dashboard.Show();
            this.Close();
        }
    }
}