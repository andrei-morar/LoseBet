using System;
using System.Windows;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class WalletWindow : Window
    {
        private string _username;
        private decimal _balance;
        private bool _bonusRevendicat = false; // Ca să nu ia bonusul de 100 de ori

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

        private void BtnDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtDepositAmount.Text, out decimal amount) && amount > 0)
            {
                // Momentan simulăm pe Frontend. 
                // Mai târziu vom apela POST api/Wallet/deposit către Backend.
                _balance += amount;
                UpdateBalanceDisplay();

                TxtStatus.Text = $"Depunere reușită: {amount} RON!";
                TxtStatus.Foreground = Brushes.Green;
                TxtDepositAmount.Text = ""; // Curățăm căsuța
            }
            else
            {
                MessageBox.Show("Suma introdusă nu este validă!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtWithdrawAmount.Text, out decimal amount) && amount > 0)
            {
                // Calculăm totalul de scăzut (suma cerută + comision 3%)
                decimal comision = amount * 0.03m;
                decimal totalDeScazut = amount + comision;

                if (totalDeScazut > _balance)
                {
                    MessageBox.Show($"Fonduri insuficiente! Cu tot cu comisionul de 3% ({comision:0.00} RON), ai nevoie de {totalDeScazut:0.00} RON în cont.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Efectuăm extragerea
                _balance -= totalDeScazut;
                UpdateBalanceDisplay();

                TxtStatus.Text = $"Ai retras {amount} RON. Comision reținut: {comision:0.00} RON.";
                TxtStatus.Foreground = Brushes.Blue;
                TxtWithdrawAmount.Text = "";
            }
            else
            {
                MessageBox.Show("Suma introdusă nu este validă!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnBonus_Click(object sender, RoutedEventArgs e)
        {
            if (!_bonusRevendicat)
            {
                _balance += 2000;
                _bonusRevendicat = true;
                UpdateBalanceDisplay();

                // Dezactivăm butonul ca să nu mai poată fi apăsat
                BtnBonus.IsEnabled = false;
                BtnBonus.Content = "Bonus revendicat ✔️";
                BtnBonus.Background = Brushes.LightGray;

                MessageBox.Show("Felicitări! Ai primit bonusul de 2000 RON!", "Bonus Acordat", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Ne întoarcem pe Dashboard cu soldul actualizat!
            DashboardWindow dashboard = new DashboardWindow(_username, _balance.ToString());
            dashboard.Show();
            this.Close();
        }
    }
}