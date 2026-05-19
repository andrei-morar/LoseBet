using System.ComponentModel;
using System.Windows;

namespace LoseBet.Desktop
{
    public partial class DashboardWindow : Window
    {
        // MAGIC TRICK: Facem variabila statică. Astfel, ține minte numele tău 
        // chiar dacă se închide și se redeschide fereastra de 100 de ori!
        private static string _savedUsername = "";

        public DashboardWindow(string username, string balance)
        {
            InitializeComponent();

            // Dacă primește un nume real, îl salvăm. Dacă primește "", îl folosește pe cel salvat anterior.
            if (!string.IsNullOrEmpty(username))
            {
                _savedUsername = username;
            }

            TxtWelcome.Text = $"Salut, {_savedUsername}!";

            // AICI AM REZOLVAT BUG-UL: Ignorăm parametrul 'balance' și aducem banii reali instant!
            RefreshDashboardBalance();

            // Verificăm rolul
            if (UserSession.Role != null && UserSession.Role.Trim().ToLower() == "admin")
            {
                BtnAdminPanel.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAdminPanel.Visibility = Visibility.Collapsed;
            }

            // Ne asigurăm că ascultă corect megafonul când se schimbă banii din alte jocuri
            GameService.BalanceUpdated -= RefreshDashboardBalance;
            GameService.BalanceUpdated += RefreshDashboardBalance;
        }

        private async void RefreshDashboardBalance()
        {
            // Aducem din nou balanța direct de la server
            decimal freshBalance = await GameService.GetBalanceAsync();

            // Adăugăm înapoi textul "Sold: " ca să arate frumos și să nu strice alte butoane
            TxtBalance.Text = $"Sold: {freshBalance:F2} RON";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void BtnSlots_Click(object sender, RoutedEventArgs e)
        {
            SlotsLobbyWindow slotsLobby = new SlotsLobbyWindow();
            slotsLobby.Show();
            this.Close();
        }

        private void BtnWallet_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            WalletWindow wallet = new WalletWindow(_savedUsername, rawBalance);
            wallet.Show();
            this.Close();
        }

        private void BtnRoulette_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            RouletteLobbyWindow rouletteLobby = new RouletteLobbyWindow(_savedUsername, rawBalance);
            rouletteLobby.Show();
            this.Close();
        }

        private void BtnBlackjack_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            BlackJackLobbyWindow blackjackLobby = new BlackJackLobbyWindow(_savedUsername, rawBalance);
            blackjackLobby.Show();
            this.Close();
        }

        private void BtnTriviador_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            TriviadorLobbyWindow triviadorLobby = new TriviadorLobbyWindow(_savedUsername, rawBalance);
            triviadorLobby.Show();
            this.Close();
        }

        // ============================
        // BUTONUL NOU: ALBA-NEAGRA
        // ============================
        private void BtnAlbaNeagra_Click(object sender, RoutedEventArgs e)
        {
            AlbaNeagraWindow albaNeagra = new AlbaNeagraWindow();
            albaNeagra.Show();
            this.Close();
        }

        private void BtnMines_Click(object sender, RoutedEventArgs e)
        {
            MinesWindow minesGame = new MinesWindow();
            minesGame.Show();
        }

        private void BtnAviator_Click(object sender, RoutedEventArgs e)
        {
            AviatorWindow aviatorGame = new AviatorWindow();
            aviatorGame.Show();
        }

        private void BtnSports_Click(object sender, RoutedEventArgs e)
        {
            SportsBettingWindow sportsWindow = new SportsBettingWindow();
            sportsWindow.Show();
            this.Close();
        }

        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow adminWindow = new AdminWindow(_savedUsername);
            adminWindow.Show();
        }
    }
}