using LoseBet.Core.Models;
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
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        public SlotsLobbyWindow()
        {
            InitializeComponent();
            RefreshBalance();
            _ = LoadGamesAsync();
        }

        private async void RefreshBalance()
        {
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"Sold: {balance:F2} RON";
        }

        private async Task LoadGamesAsync()
        {
            try
            {
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
                SlotEngineWindow engine = new SlotEngineWindow(selectedGame);
                engine.Show();
                this.Close();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Am scos paramentrii din constructor (deoarece cred ca nu ii mai folosești, avand in vedere GameService)
            // Dacă constructorul tău curent din DashboardWindow cere parametrii, păstrează-i pe cei vechi.
            DashboardWindow dashboard = new DashboardWindow("", "");
            dashboard.Show();
            this.Close();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshBalance();
            _ = LoadGamesAsync();
        }
    }
}