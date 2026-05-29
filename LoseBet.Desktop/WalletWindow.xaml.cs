using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class WalletWindow : Window
    {
        private string _username;
        private decimal _balance;
        private bool _bonusRevendicat = false;
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        public WalletWindow(string username, string currentBalance)
        {
            InitializeComponent();
            _username = username;

            if (decimal.TryParse(currentBalance, out decimal bal))
            {
                _balance = bal;
            }

            UpdateBalanceDisplay();
        }

        private void UpdateBalanceDisplay()
        {
            TxtBalance.Text = $"{_balance:0.00} RON";
        }

        private async Task SaveBalanceToDatabase()
        {
            try
            {
                var updateData = new { Username = _username, NewBalance = _balance };
                var response = await _httpClient.PostAsJsonAsync("api/auth/update-balance", updateData);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Atenție: Balanța nu a putut fi salvată în cloud!", "Eroare Sincronizare");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Eroare conexiune server. Verifică conexiunea la internet.");
            }
        }

        // ==========================================
        // LOGICA PENTRU PACHETELE PREDEFINITE
        // ==========================================
        private async void BtnPackage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                // Tag-ul este în formatul "SumaDepusa|Bonus" (ex: "100|20")
                string[] parts = btn.Tag.ToString()!.Split('|');

                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out decimal deposit) &&
                    decimal.TryParse(parts[1], out decimal bonus))
                {
                    _balance += (deposit + bonus);
                    UpdateBalanceDisplay();

                    await SaveBalanceToDatabase();

                    string msg = bonus > 0 ? $"Ai depus {deposit} RON și ai primit {bonus} RON BONUS!" : $"Depunere reușită: {deposit} RON!";
                    SetStatus(msg, "#00E676"); // Verde
                }
            }
        }

        // ==========================================
        // DEPUNERE MANUALĂ
        // ==========================================
        private async void BtnDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtDepositAmount.Text, out decimal amount) && amount > 0)
            {
                _balance += amount;
                UpdateBalanceDisplay();

                await SaveBalanceToDatabase();

                SetStatus($"Depunere manuală reușită: {amount} RON!", "#00E676");
                TxtDepositAmount.Text = "";
            }
            else
            {
                SetStatus("Introdu o sumă validă pentru depunere!", "#FF5252");
            }
        }

        // ==========================================
        // RETRAGERE (CU COMISION)
        // ==========================================
        private async void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtWithdrawAmount.Text, out decimal amount) && amount > 0)
            {
                decimal comision = amount * 0.03m;
                decimal totalDeScazut = amount + comision;

                if (totalDeScazut > _balance)
                {
                    SetStatus("Fonduri insuficiente pentru retragere + comision!", "#FF5252");
                    return;
                }

                _balance -= totalDeScazut;
                UpdateBalanceDisplay();

                await SaveBalanceToDatabase();

                SetStatus($"Retragere reușită! Au fost deduși {totalDeScazut:F2} RON (inclusiv comision 3%).", "#FFD700");
                TxtWithdrawAmount.Text = "";
            }
            else
            {
                SetStatus("Introdu o sumă validă pentru retragere!", "#FF5252");
            }
        }

        // ==========================================
        // BONUS BUN VENIT 2000 RON
        // ==========================================
        private async void BtnBonus_Click(object sender, RoutedEventArgs e)
        {
            if (!_bonusRevendicat)
            {
                _balance += 2000;
                _bonusRevendicat = true;
                UpdateBalanceDisplay();

                await SaveBalanceToDatabase();

                BtnBonus.IsEnabled = false;
                BtnBonus.Content = "BONUS REVENDICAT ✔️";
                BtnBonus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
                BtnBonus.Foreground = Brushes.Gray;

                SetStatus("Felicitări! Ai revendicat bonusul de bun venit de 2000 RON!", "#00E676");
            }
        }

        private void SetStatus(string text, string hexColor)
        {
            TxtStatus.Text = text;
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Ne întoarcem la dashboard, pasând înapoi datele actualizate
            DashboardWindow dashboard = new DashboardWindow(_username, _balance.ToString("F2"));
            dashboard.Show();
            this.Close();
        }
    }
}