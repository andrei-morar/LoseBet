using LoseBet.Core.Models; // Importăm modelul SlotGame
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LoseBet.Desktop
{
    public partial class SlotsLobbyWindow : Window
    {
        private string _username;
        private string _balance;
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        public SlotsLobbyWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            _balance = balance;
            TxtBalance.Text = $"Sold: {balance} RON";

            _ = LoadGamesAsync();
        }

        private async Task LoadGamesAsync()
        {
            try
            {
                // Chemăm API-ul să ne dea jocurile active
                var games = await _httpClient.GetFromJsonAsync<List<SlotGame>>("api/slots/active");
                if (games != null)
                {
                    ItemsGames.ItemsSource = games;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea jocurilor: " + ex.Message);
            }
        }

        private void BtnPlayGame_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedGame = button?.Tag as SlotGame;

            if (selectedGame != null)
            {
                // Deschidem motorul de joc și îi dăm tot ce are nevoie
                SlotEngineWindow engine = new SlotEngineWindow(_username, _balance, selectedGame);
                engine.Show();
                this.Close();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_username, _balance);
            dashboard.Show();
            this.Close();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadGamesAsync();
        }
    }
}