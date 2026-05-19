using LoseBet.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LoseBet.Desktop
{
    public partial class SportsBettingWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        // Lista cu ce a selectat omul pe bilet (o folosim pentru a o afișa în dreapta)
        private ObservableCollection<BetSelectionUI> _betSlip = new ObservableCollection<BetSelectionUI>();

        public SportsBettingWindow()
        {
            InitializeComponent();
            LstBetSlip.ItemsSource = _betSlip;

            RefreshBalance();
            _ = LoadMatchesAsync();
            _ = LoadTicketsAsync();
        }

        private async void RefreshBalance()
        {
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"{balance:F2} RON";
        }

        private async Task LoadMatchesAsync()
        {
            try
            {
                var matches = await _httpClient.GetFromJsonAsync<List<SportsMatch>>("api/sports/active-matches");
                if (matches != null) ItemsMatches.ItemsSource = matches;
            }
            catch { }
        }

        private async Task LoadTicketsAsync()
        {
            try
            {
                var tickets = await _httpClient.GetFromJsonAsync<List<BetTicket>>($"api/sports/tickets/{UserSession.UserId}");
                if (tickets != null)
                {
                    // Am schimbat aici: folosim noul TicketsList creat în XAML!
                    TicketsList.ItemsSource = tickets;
                }
            }
            catch { }
        }

        // Când jucătorul dă click pe o cotă (1, X sau 2)
        private void BtnOdd_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var match = btn?.DataContext as SportsMatch;
            string pick = btn?.Tag.ToString(); // "1", "X" sau "2"

            if (match != null && pick != null)
            {
                decimal odds = pick == "1" ? match.Odds1 : (pick == "X" ? match.OddsX : match.Odds2);

                // Verificăm dacă a pariat deja pe meciul ăsta. Dacă da, îi actualizăm pronosticul (nu lăsăm 2 pronosticuri pe același meci)
                var existingSelection = _betSlip.FirstOrDefault(s => s.MatchId == match.Id);
                if (existingSelection != null)
                {
                    _betSlip.Remove(existingSelection);
                }

                _betSlip.Add(new BetSelectionUI
                {
                    MatchId = match.Id,
                    MatchName = $"{match.HomeTeam} - {match.AwayTeam}",
                    Pick = pick,
                    Odds = odds
                });

                CalculateSlip();
            }
        }

        // Ștergere de pe bilet
        private void BtnRemoveSelection_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int matchId = (int)btn.Tag;

            var item = _betSlip.FirstOrDefault(s => s.MatchId == matchId);
            if (item != null)
            {
                _betSlip.Remove(item);
                CalculateSlip();
            }
        }

        // Calculează cota totală și câștigul
        private void CalculateSlip()
        {
            // PROTECȚIA AICI: Dacă interfața nu s-a încărcat complet, ne oprim.
            if (TxtTotalOdds == null || TxtPotentialWin == null || TxtStake == null)
                return;

            decimal totalOdds = 1;
            foreach (var item in _betSlip)
            {
                totalOdds *= item.Odds;
            }

            // Dacă biletul e gol, cota e 0 vizual
            if (_betSlip.Count == 0) totalOdds = 0;

            TxtTotalOdds.Text = totalOdds.ToString("0.00");

            if (decimal.TryParse(TxtStake.Text, out decimal stake))
            {
                TxtPotentialWin.Text = (stake * totalOdds).ToString("0.00");
            }
        }

        private void TxtStake_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateSlip();
        }

        // Plasarea biletului în baza de date
        private async void BtnPlaceBet_Click(object sender, RoutedEventArgs e)
        {
            if (_betSlip.Count == 0)
            {
                MessageBox.Show("Biletul este gol! Adaugă meciuri înainte de a paria.");
                return;
            }

            if (!decimal.TryParse(TxtStake.Text, out decimal stake) || stake <= 0)
            {
                MessageBox.Show("Introdu o miză validă!");
                return;
            }

            BtnPlaceBet.IsEnabled = false;

            try
            {
                // Pregătim datele pentru API
                var payload = new
                {
                    UserId = UserSession.UserId,
                    Stake = stake,
                    Selections = _betSlip.Select(s => new { MatchId = s.MatchId, Pick = s.Pick }).ToList()
                };

                var res = await _httpClient.PostAsJsonAsync("api/sports/place-bet", payload);

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Bilet plasat cu succes! Baftă!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                    _betSlip.Clear();
                    CalculateSlip();
                    RefreshBalance();
                    await LoadTicketsAsync(); // Actualizăm lista de bilete din celălalt tab
                }
                else
                {
                    MessageBox.Show(await res.Content.ReadAsStringAsync(), "Eroare");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare de conexiune: " + ex.Message);
            }

            BtnPlaceBet.IsEnabled = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow("", "");
            dashboard.Show();
            this.Close();
        }
    }

    // Clasă pentru a ține datele temporare pe biletul din UI
    public class BetSelectionUI
    {
        public int MatchId { get; set; }
        public string MatchName { get; set; } = string.Empty;
        public string Pick { get; set; } = string.Empty;
        public decimal Odds { get; set; }
    }
}