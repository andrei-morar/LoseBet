using System.Windows;

namespace LoseBet.Desktop
{
    public partial class DashboardWindow : Window
    {
        // 1. AM ADĂUGAT VARIABILA AICI:
        private string _username;

        // Constructorul care primește username și sold
        public DashboardWindow(string username, string balance)
        {
            InitializeComponent();

            // 2. AM SALVAT USERNAME-UL CA SĂ-L ȘTIE TOATĂ CLASA:
            _username = username;

            // Setăm textele pe ecran
            TxtWelcome.Text = $"Salut, {username}!";
            TxtBalance.Text = $"Sold: {balance} RON";

            // 3. AICI ESTE MAGIA PENTRU BUTONUL DE ADMIN:
            // Verificăm dacă rolul din sesiune este "admin" (indiferent cum e scris în baza de date)
            if (UserSession.Role != null && UserSession.Role.Trim().ToLower() == "admin")
            {
                BtnAdminPanel.Visibility = Visibility.Visible; // Ești șef, primești butonul!
            }
            else
            {
                BtnAdminPanel.Visibility = Visibility.Collapsed; // Ești jucător, butonul e ascuns!
            }
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
            // Acum poți folosi direct _username în loc să-l mai decupezi cu Replace!
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");

            // Deschidem Lobby-ul de Păcănele
            SlotsLobbyWindow lobby = new SlotsLobbyWindow(_username, rawBalance);
            lobby.Show();
            this.Close();
        }

        private void BtnWallet_Click(object sender, RoutedEventArgs e)
        {
            // Deschidem Casieria (Portofelul)
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");

            WalletWindow wallet = new WalletWindow(_username, rawBalance);
            wallet.Show();
            this.Close();
        }

        private void BtnRoulette_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");

            RouletteLobbyWindow rouletteLobby = new RouletteLobbyWindow(_username, rawBalance);
            rouletteLobby.Show();
            this.Close();
        }

        private void BtnBlackjack_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");

            BlackJackLobbyWindow blackjackLobby = new BlackJackLobbyWindow(_username, rawBalance);
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

        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            // Acum funcționează perfect, deoarece _username este recunoscut!
            AdminWindow adminWindow = new AdminWindow(_username);
            adminWindow.Show();
        }
    }
}