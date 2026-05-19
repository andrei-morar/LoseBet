using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using System.Net.Http.Json; // Foarte important pentru PostAsJsonAsync
using System.Threading.Tasks;

namespace LoseBet.Desktop
{
    public partial class WalletWindow : Window
    {
        private string _username;
        private decimal _balance;
        private bool _bonusRevendicat = false;
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") }; // ASIGURĂ-TE CĂ PORTUL E CORECT (vezi în API)

        public WalletWindow(string username, string currentBalance)
        {
            InitializeComponent();
            _username = username;
            decimal.TryParse(currentBalance, out _balance);
            UpdateBalanceDisplay();
        }

        private void UpdateBalanceDisplay()
        {
            TxtBalance.Text = $"Sold: {_balance:0.00} RON";
        }

        // Metodă ajutătoare pentru a salva în Supabase
        private async Task SaveBalanceToDatabase()
        {
            try
            {
                var updateData = new { Username = _username, NewBalance = _balance };
                var response = await _httpClient.PostAsJsonAsync("api/auth/update-balance", updateData);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Atenție: Balanța nu a putut fi salvată în cloud!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare conexiune server: " + ex.Message);
            }
        }

        private async void BtnDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtDepositAmount.Text, out decimal amount) && amount > 0)
            {
                _balance += amount;
                UpdateBalanceDisplay();

                // SALVĂM ÎN CLOUD
                await SaveBalanceToDatabase();

                TxtStatus.Text = $"Depunere reușită: {amount} RON!";
                TxtStatus.Foreground = Brushes.Green;
                TxtDepositAmount.Text = "";
            }
        }

        private async void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtWithdrawAmount.Text, out decimal amount) && amount > 0)
            {
                decimal comision = amount * 0.03m;
                decimal totalDeScazut = amount + comision;

                if (totalDeScazut > _balance)
                {
                    MessageBox.Show("Fonduri insuficiente!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _balance -= totalDeScazut;
                UpdateBalanceDisplay();

                // SALVĂM ÎN CLOUD
                await SaveBalanceToDatabase();

                TxtStatus.Text = $"Retragere reușită!";
                TxtStatus.Foreground = Brushes.Blue;
                TxtWithdrawAmount.Text = "";
            }
        }

        private async void BtnBonus_Click(object sender, RoutedEventArgs e)
        {
            if (!_bonusRevendicat)
            {
                _balance += 2000;
                _bonusRevendicat = true;
                UpdateBalanceDisplay();

                // SALVĂM ÎN CLOUD
                await SaveBalanceToDatabase();

                BtnBonus.IsEnabled = false;
                BtnBonus.Content = "Bonus revendicat ✔️";
                MessageBox.Show("Ai primit bonusul de 2000 RON și a fost salvat în cloud!", "Succes");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_username, _balance.ToString());
            dashboard.Show();
            this.Close();
        }
    }
}