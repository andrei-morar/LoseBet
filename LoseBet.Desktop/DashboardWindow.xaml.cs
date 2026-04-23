using System.Windows;

namespace LoseBet.Desktop
{
    public partial class DashboardWindow : Window
    {
        // Constructorul care primește username și sold
        public DashboardWindow(string username, string balance)
        {
            InitializeComponent();

            // Setăm textele pe ecran
            TxtWelcome.Text = $"Salut, {username}!";
            TxtBalance.Text = $"Sold: {balance} RON";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Când dă logout, redeschidem fereastra de Login
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();

            // Închidem Dashboard-ul actual
            this.Close();
        }

        private void BtnSlots_Click(object sender, RoutedEventArgs e)
        {
            // Luăm datele de pe ecran ca să le pasăm mai departe
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            string username = TxtWelcome.Text.Replace("Salut, ", "").Replace("!", "");

            // Deschidem Lobby-ul de Păcănele
            SlotsLobbyWindow lobby = new SlotsLobbyWindow(username, rawBalance);
            lobby.Show();
            this.Close();
        }

        private void BtnWallet_Click(object sender, RoutedEventArgs e)
        {
            // Deschidem Casieria (Portofelul)
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            string username = TxtWelcome.Text.Replace("Salut, ", "").Replace("!", "");

            WalletWindow wallet = new WalletWindow(username, rawBalance);
            wallet.Show();
            this.Close();
        }
        private void BtnRoulette_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            string username = TxtWelcome.Text.Replace("Salut, ", "").Replace("!", "");

            RouletteLobbyWindow rouletteLobby = new RouletteLobbyWindow(username, rawBalance);
            rouletteLobby.Show();
            this.Close();
        }
        private void BtnBlackjack_Click(object sender, RoutedEventArgs e)
        {
            // Luăm datele de pe ecran ca să le pasăm mai departe
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            string username = TxtWelcome.Text.Replace("Salut, ", "").Replace("!", "");

            // AICI ERA PROBLEMA! Trebuie cu J mare ca să găsească fișierul corect
            BlackJackLobbyWindow blackjackLobby = new BlackJackLobbyWindow(username, rawBalance);
            blackjackLobby.Show();
            this.Close();
        }
        // ==========================================
        // BUTOANELE NOI (NEIMPLEMENTATE MOMENTAN)
        // ==========================================

        private void BtnSports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Modulul 'Pariuri Sportive' nu a fost încă implementat. În curând!", "În dezvoltare", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}