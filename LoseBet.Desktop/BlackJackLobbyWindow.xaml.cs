using System;
using System.Windows;

namespace LoseBet.Desktop
{
    public partial class BlackJackLobbyWindow : Window
    {
        // Punem '?' ca să eliminăm eroarea de Nullable din Visual Studio/Git
        private string? _username;
        private string? _balance;

        public BlackJackLobbyWindow(string? username, string? balance)
        {
            InitializeComponent();

            // Dacă din vreo eroare vine null, punem valori de rezervă
            _username = username ?? "Guest";
            _balance = balance ?? "0.00";

            // Afișăm soldul pe ecran
            TxtBalance.Text = $"Sold: {_balance} RON";
        }

        private void BtnPlayClassic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Trimitem datele curate către fereastra de joc
                BlackjackGameWindow game = new BlackjackGameWindow(_username, _balance);
                game.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la deschiderea mesei de joc: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPlayVIP_Click(object sender, RoutedEventArgs e)
        {
            // Am păstrat logica ta, dar i-am dat un text mai "premium"
            MessageBox.Show("Masa VIP este rezervată momentan pentru jucătorii High Roller. Mai crește soldul!", "VIP Exclusive", MessageBoxButton.OK, MessageBoxImage.Information);

            /* * Dacă vrei mai târziu să o deblochezi, ștergi MessageBox-ul și pui asta:
             * BlackjackGameWindow game = new BlackjackGameWindow(_username, _balance);
             * game.Show();
             * this.Close();
             */
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ne întoarcem înapoi la Dashboard
                DashboardWindow dashboard = new DashboardWindow(_username, _balance);
                dashboard.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la întoarcerea în Dashboard: " + ex.Message);
                this.Close();
            }
        }
    }
}