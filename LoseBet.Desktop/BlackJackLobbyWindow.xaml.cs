using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace LoseBet.Desktop
{
    public partial class BlackJackLobbyWindow : Window
    {
        private string? _username;
        private string? _balance;

        public BlackJackLobbyWindow(string? username, string? balance)
        {
            InitializeComponent();
            _username = username ?? "Guest";
            _balance = balance ?? "0.00";
            TxtBalance.Text = $"Sold: {_balance} RON";
        }

        private void OpenWalletWithPrefill(int amount)
        {
            try
            {
                WalletWindow wallet = new WalletWindow(_username ?? "Guest", _balance ?? "0.00", amount);
                wallet.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la deschiderea Casieriei: " + ex.Message);
            }
        }

        // bonus claim moved to Wallet; no local handler here

        private void BtnPlayClassic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Masa clasică
                BlackjackGameWindow game = new BlackjackGameWindow(_username, _balance, false);
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
            try
            {
                // FĂRĂ NICIO VERIFICARE! Te bagă DIRECT la masa VIP (parametrul 'true')
                BlackjackGameWindow game = new BlackjackGameWindow(_username, _balance, true);
                game.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la deschiderea mesei VIP: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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