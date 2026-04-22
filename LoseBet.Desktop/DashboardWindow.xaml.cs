using System.Windows;

namespace LoseBet.Desktop
{
    public partial class DashboardWindow : Window
    {
        // Modificăm constructorul să primească username și sold
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

            // Și închidem Dashboard-ul actual
            this.Close();
        }
    }
}