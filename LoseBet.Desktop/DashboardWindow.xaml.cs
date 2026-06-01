using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LoseBet.Desktop
{
    public partial class DashboardWindow : Window
    {
        private static string _savedUsername = "";

        // Câți pixeli sare banda glisantă la un click
        private const double SCROLL_STEP = 200.0;

        public DashboardWindow(string username, string balance)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(username))
            {
                _savedUsername = username;
            }

            TxtWelcome.Text = $"Salut, {_savedUsername}!";
            RefreshDashboardBalance();

            if (UserSession.Role != null && UserSession.Role.Trim().ToLower() == "admin")
            {
                BtnAdminPanel.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAdminPanel.Visibility = Visibility.Collapsed;
            }

            GameService.BalanceUpdated -= RefreshDashboardBalance;
            GameService.BalanceUpdated += RefreshDashboardBalance;
        }

        private async void RefreshDashboardBalance()
        {
            try
            {
                decimal freshBalance = await GameService.GetBalanceAsync();
                TxtBalance.Text = $"Sold: {freshBalance:F2} RON";
            }
            catch (Exception) // Am scos 'ex' ca să dispară warning-ul
            {
                // Eroare ignorată silențios dacă pică serverul momentan
            }
        }

        // ============================
        // ANIMAȚIE GLISARE (SLIDER) - METODELE CARE LIPSEAU
        // ============================

        private async void AnimateScroll(ScrollViewer viewer, double offsetChange)
        {
            double targetOffset = viewer.HorizontalOffset + offsetChange;
            double step = offsetChange / 10.0; // Împărțim animația în 10 cadre (frames)

            for (int i = 0; i < 10; i++)
            {
                viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset + step);
                await Task.Delay(15); // Așteaptă 15 milisecunde pt finețe
            }
        }

        private void BtnScrollEduLeft_Click(object sender, RoutedEventArgs e)
        {
            AnimateScroll(ScrollEdu, -SCROLL_STEP);
        }

        private void BtnScrollEduRight_Click(object sender, RoutedEventArgs e)
        {
            AnimateScroll(ScrollEdu, SCROLL_STEP);
        }

        private void BtnScrollCasinoLeft_Click(object sender, RoutedEventArgs e)
        {
            AnimateScroll(ScrollCasino, -SCROLL_STEP);
        }

        private void BtnScrollCasinoRight_Click(object sender, RoutedEventArgs e)
        {
            AnimateScroll(ScrollCasino, SCROLL_STEP);
        }


        // ============================
        // LOGICA PENTRU MENIUL LATERAL
        // ============================

        private void BtnMenuHome_Click(object sender, RoutedEventArgs e)
        {
            PromoBanner.Visibility = Visibility.Visible;
            PanelEdu.Visibility = Visibility.Visible;
            PanelCasino.Visibility = Visibility.Visible;
        }

        private void BtnMenuEdu_Click(object sender, RoutedEventArgs e)
        {
            PromoBanner.Visibility = Visibility.Collapsed;
            PanelEdu.Visibility = Visibility.Visible;
            PanelCasino.Visibility = Visibility.Collapsed;
        }

        private void BtnMenuCasino_Click(object sender, RoutedEventArgs e)
        {
            PromoBanner.Visibility = Visibility.Collapsed;
            PanelEdu.Visibility = Visibility.Collapsed;
            PanelCasino.Visibility = Visibility.Visible;
        }

        private void BtnMenuLive_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Secțiunea Live Casino va fi disponibilă în curând!", "În dezvoltare");
        }

        private void BtnMenuSports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Secțiunea Pariuri Sportive va fi disponibilă în curând!", "În dezvoltare");
        }

        // ============================
        // BARA DE SUS (HEADER)
        // ============================

        private void BtnWallet_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            WalletWindow wallet = new WalletWindow(_savedUsername, rawBalance);
            wallet.Show();
            this.Close();
        }

        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow adminWindow = new AdminWindow(_savedUsername);
            adminWindow.Show();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        // ============================
        // DESCHIDERE JOCURI
        // ============================

        private void BtnSlots_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SlotsLobbyWindow slotsLobby = new SlotsLobbyWindow();
                slotsLobby.Show();
                this.Close();
            }
            catch (Exception) { } // Fără 'ex'
        }

        private void BtnRoulette_Click(object sender, RoutedEventArgs e)
        {
            string rawBalance = TxtBalance.Text.Replace("Sold: ", "").Replace(" RON", "");
            // Apelăm direct jocul de ruletă (european), sărind peste Lobby-ul cu opțiunea americană scoasă
            RouletteGameWindow rouletteGame = new RouletteGameWindow(_savedUsername, rawBalance);
            rouletteGame.Show();
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

        private void BtnWordle_Click(object sender, RoutedEventArgs e)
        {
            WordleWindow wordleGame = new WordleWindow();
            wordleGame.Show();
        }

        private void BtnCifre_Click(object sender, RoutedEventArgs e)
        {
            CifreWindow cifreGame = new CifreWindow();
            cifreGame.Show();
        }
    }
}