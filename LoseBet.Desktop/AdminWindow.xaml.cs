using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using LoseBet.Core.Models;
using System.Linq;

namespace LoseBet.Desktop
{
    public partial class AdminWindow : Window
    {
        private readonly string _adminUsername;
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        // Acum folosim lista de simboluri configurate cu multiplicatori!
        private List<SymbolConfigDTO> _configuredSymbols = new List<SymbolConfigDTO>();
        private int? _selectedSlotId = null;

        public AdminWindow(string adminUsername)
        {
            InitializeComponent();
            _adminUsername = adminUsername;
            _ = LoadUsersAsync();
            _ = LoadSlotsAsync();
        }

        private async Task LoadUsersAsync()
        {
            var res = await _httpClient.GetAsync($"api/admin/users?adminUsername={_adminUsername}");
            if (res.IsSuccessStatusCode) UsersGrid.ItemsSource = await res.Content.ReadFromJsonAsync<List<UserDto>>();
        }

        private async Task LoadSlotsAsync()
        {
            var res = await _httpClient.GetAsync("api/slots/active");
            if (res.IsSuccessStatusCode)
            {
                SlotsGrid.ItemsSource = await res.Content.ReadFromJsonAsync<List<SlotGame>>();
            }
        }

        // --- GESTIUNE USERS ---
        private async void BtnToggleBan_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserDto u)
            {
                await _httpClient.PostAsJsonAsync("api/admin/toggle-ban", new { AdminUsername = _adminUsername, TargetUsername = u.Username });
                await LoadUsersAsync();
            }
        }

        private async void BtnUpdateRole_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserDto u && CmbRoles.SelectedItem is ComboBoxItem i)
            {
                await _httpClient.PostAsJsonAsync("api/admin/update-role", new { AdminUsername = _adminUsername, TargetUsername = u.Username, NewRole = i.Content.ToString() });
                await LoadUsersAsync();
            }
        }

        private async void BtnUpdateMoney_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserDto u && decimal.TryParse(TxtAmount.Text, out decimal amt))
            {
                await _httpClient.PostAsJsonAsync("api/admin/update-balance", new { AdminUsername = _adminUsername, TargetUsername = u.Username, NewBalance = amt });
                await LoadUsersAsync();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => _ = LoadUsersAsync();

        // --- GESTIUNE SLOTS ---
        private void SlotsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid?.SelectedItem is SlotGame s)
            {
                _selectedSlotId = s.Id;
                TxtSlotName.Text = s.Name;
                TxtSlotThumb.Text = s.ThumbnailUrl;
                TxtSlotColor.Text = s.ThemeColor;
                TxtSlotRows.Text = s.Rows.ToString();
                TxtSlotCols.Text = s.Columns.ToString();
                TxtSlotPaylines.Text = s.Paylines.ToString();

                // Curățăm lista curentă
                _configuredSymbols.Clear();

                // Dacă jocul selectat are simboluri din baza de date, le încărcăm vizual!
                if (s.Symbols != null)
                {
                    foreach (var sym in s.Symbols)
                    {
                        _configuredSymbols.Add(new SymbolConfigDTO
                        {
                            ImageName = sym.ImageName,
                            Multiplier3 = sym.Multiplier3,
                            Multiplier4 = sym.Multiplier4,
                            Multiplier5 = sym.Multiplier5,
                            IsWild = sym.IsWild
                        });
                    }
                }

                // Dăm refresh la ListBox ca să apară pe ecran
                LstConfiguredSymbols.ItemsSource = null;
                LstConfiguredSymbols.ItemsSource = _configuredSymbols;

                BtnUploadSymbols.Content = $"📁 Adăugă Simbol Nou ({_configuredSymbols.Count})";
                BtnUploadSymbols.Background = _configuredSymbols.Count > 0
                    ? System.Windows.Media.Brushes.MediumSeaGreen
                    : new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#444"));
            }
        }

        private async void BtnDeleteSlot_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSlotId != null)
            {
                await _httpClient.DeleteAsync($"api/slots/delete/{_selectedSlotId}");
                await LoadSlotsAsync();
                BtnNewMode_Click(null, null);
            }
        }

        private void BtnNewMode_Click(object sender, RoutedEventArgs e)
        {
            _selectedSlotId = null;
            TxtSlotName.Text = "";
            TxtSlotThumb.Text = "Selectează o poză...";
            TxtSlotColor.Text = "#1E1E1E";

            _configuredSymbols.Clear();
            LstConfiguredSymbols.ItemsSource = null; // <-- ADAUGĂ LINIA ASTA

            BtnUploadSymbols.Content = "➕ Adaugă Simbol Nou";
            BtnUploadSymbols.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#444"));

            MessageBox.Show("Mod creare joc nou activat. Poți adăuga un joc de la zero.");
        }

        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Imagini (*.png;*.jpg)|*.png;*.jpg" };
            if (op.ShowDialog() == true) TxtSlotThumb.Text = CopyToResources(op.FileName);
        }

        private void BtnUploadSymbols_Click(object sender, RoutedEventArgs e)
        {
            SymbolConfigWindow symWindow = new SymbolConfigWindow();
            symWindow.Owner = this;

            if (symWindow.ShowDialog() == true && symWindow.ConfiguredSymbol != null)
            {
                _configuredSymbols.Add(symWindow.ConfiguredSymbol);

                // Sincronizăm lista vizuală din interfață
                LstConfiguredSymbols.ItemsSource = null;
                LstConfiguredSymbols.ItemsSource = _configuredSymbols;

                BtnUploadSymbols.Content = $"📁 Adăugă Simbol Nou ({_configuredSymbols.Count})";
                BtnUploadSymbols.Background = System.Windows.Media.Brushes.MediumSeaGreen;
            }
        }

        private void BtnRemoveSymbolFromList_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var symbolToRemove = button?.Tag as SymbolConfigDTO;

            if (symbolToRemove != null)
            {
                _configuredSymbols.Remove(symbolToRemove);

                // Refresh la listă
                LstConfiguredSymbols.ItemsSource = null;
                LstConfiguredSymbols.ItemsSource = _configuredSymbols;

                BtnUploadSymbols.Content = _configuredSymbols.Count > 0
                    ? $"📁 Adăugă Simbol Nou ({_configuredSymbols.Count})"
                    : "➕ Adaugă Simbol Nou";

                if (_configuredSymbols.Count == 0)
                {
                    BtnUploadSymbols.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#444"));
                }
            }
        }

        private async void BtnAddSlot_Click(object sender, RoutedEventArgs e)
        {
            if (_configuredSymbols.Count < 3)
            {
                MessageBox.Show("Te rog adaugă măcar 3 simboluri configurate pentru ca jocul să poată plăti!");
                return;
            }

            var data = new
            {
                Name = TxtSlotName.Text,
                ThumbnailUrl = TxtSlotThumb.Text,
                ThemeColor = TxtSlotColor.Text,
                Rows = int.Parse(TxtSlotRows.Text),
                Columns = int.Parse(TxtSlotCols.Text),
                Paylines = int.Parse(TxtSlotPaylines.Text),
                WildType = ((ComboBoxItem)CmbWildType.SelectedItem).Content.ToString(),
                Symbols = _configuredSymbols // Trimitem simbolurile complexe!
            };

            if (_selectedSlotId == null)
            {
                await _httpClient.PostAsJsonAsync("api/slots/add", data);
            }
            else
            {
                await _httpClient.PutAsJsonAsync($"api/slots/update/{_selectedSlotId}", data);
            }

            await LoadSlotsAsync();
            BtnNewMode_Click(null, null);
            BtnUploadSymbols.Content = "📁 Încarcă Simboluri (Multi)";
            BtnUploadSymbols.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#444"));

            MessageBox.Show("Jocul a fost salvat cu succes în baza de date!");
        }

        private string CopyToResources(string src)
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Slots");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string name = Path.GetFileName(src);
            File.Copy(src, Path.Combine(folder, name), true);
            return name;
        }

        // ==========================================
        // MODULUL DE PARIURI SPORTIVE
        // ==========================================

        private async Task LoadMatchesAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync("api/sports/active-matches");
                if (res.IsSuccessStatusCode)
                {
                    MatchesGrid.ItemsSource = await res.Content.ReadFromJsonAsync<List<SportsMatch>>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la aducerea meciurilor: " + ex.Message);
            }
        }

        private async void BtnAddMatch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHomeTeam.Text) || string.IsNullOrWhiteSpace(TxtAwayTeam.Text))
            {
                MessageBox.Show("Completează numele ambelor echipe!");
                return;
            }

            var data = new
            {
                HomeTeam = TxtHomeTeam.Text,
                AwayTeam = TxtAwayTeam.Text,
                Odds1 = decimal.Parse(TxtOdds1.Text),
                OddsX = decimal.Parse(TxtOddsX.Text),
                Odds2 = decimal.Parse(TxtOdds2.Text)
            };

            var res = await _httpClient.PostAsJsonAsync("api/sports/admin/add-match", data);

            if (res.IsSuccessStatusCode)
            {
                MessageBox.Show("Meciul a fost adăugat în oferta casei de pariuri!");
                TxtHomeTeam.Text = "";
                TxtAwayTeam.Text = "";
                TxtOdds1.Text = "1.00";
                TxtOddsX.Text = "1.00";
                TxtOdds2.Text = "1.00";
                await LoadMatchesAsync(); // Refresh la grilă
            }
        }

        private void BtnRefreshMatches_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadMatchesAsync();
        }

        private async void BtnSimulateMatch_Click(object sender, RoutedEventArgs e)
        {
            if (MatchesGrid.SelectedItem is SportsMatch selectedMatch)
            {
                var result = MessageBox.Show($"Ești sigur că vrei să simulezi meciul {selectedMatch.HomeTeam} - {selectedMatch.AwayTeam}?\n\nScorul va fi generat aleator, iar biletele vor fi plătite!", "Simulare Meci", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var res = await _httpClient.PostAsync($"api/sports/admin/simulate/{selectedMatch.Id}", null);

                        if (res.IsSuccessStatusCode)
                        {
                            // Citim mesajul (care conține scorul generat)
                            var responseData = await res.Content.ReadFromJsonAsync<SimulateResponseDTO>();
                            MessageBox.Show(responseData?.Message, "Fluier final!", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Dăm refresh la grilă (meciul ar trebui să dispară pentru că acum e "Finished")
                            await LoadMatchesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la simulare: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Te rog să selectezi un meci din tabel pentru a-l simula!");
            }
        }
        // Ascunde sau arată variantele în funcție de tipul ales
        private void CmbTriviaType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelTriviaOptions == null) return; // Evităm crash-ul la încărcarea inițială

            var selected = (ComboBoxItem)CmbTriviaType.SelectedItem;
            if (selected.Tag.ToString() == "1") // 1 = Aproximare
            {
                PanelTriviaOptions.Visibility = Visibility.Collapsed;
            }
            else // 2 = Grilă
            {
                PanelTriviaOptions.Visibility = Visibility.Visible;
            }
        }

        // Adaugă întrebarea în baza de date
        private async void BtnAddTrivia_Click(object sender, RoutedEventArgs e)
        {
            var selectedType = (ComboBoxItem)CmbTriviaType.SelectedItem;
            int type = int.Parse(selectedType.Tag.ToString());

            var newQuestion = new LoseBet.Core.Models.TriviaQuestion
            {
                // Înlocuiește linia veche TxtTriviaCategory.Text.Trim() cu asta:
                Category = (CmbTriviaCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Diverse",
                Text = TxtTriviaText.Text.Trim(),
                Type = type,
                CorrectAnswer = TxtTriviaCorrect.Text.Trim(),
                OptionA = type == 2 ? TxtTriviaA.Text.Trim() : null,
                OptionB = type == 2 ? TxtTriviaB.Text.Trim() : null,
                OptionC = type == 2 ? TxtTriviaC.Text.Trim() : null,
                OptionD = type == 2 ? TxtTriviaD.Text.Trim() : null
            };

            if (string.IsNullOrEmpty(newQuestion.Text) || string.IsNullOrEmpty(newQuestion.CorrectAnswer))
            {
                MessageBox.Show("Textul întrebării și răspunsul corect sunt obligatorii!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/trivia/admin/add", newQuestion);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Întrebare adăugată cu succes în baza de date!", "Succes");
                    // Curățăm câmpurile
                    TxtTriviaText.Clear(); TxtTriviaCorrect.Clear();
                    TxtTriviaA.Clear(); TxtTriviaB.Clear(); TxtTriviaC.Clear(); TxtTriviaD.Clear();
                }
                else
                {
                    MessageBox.Show("Eroare la adăugarea întrebării pe server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare de conexiune: " + ex.Message);
            }
        }

        // 1. Încărcare listă
        private async void BtnRefreshTrivia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var questions = await _httpClient.GetFromJsonAsync<List<LoseBet.Core.Models.TriviaQuestion>>("api/trivia/questions");
                TriviaGrid.ItemsSource = questions;
            }
            catch { MessageBox.Show("Eroare la încărcarea întrebărilor."); }
        }

        // 2. Ștergere (trebuie să adaugi un endpoint în TriviaController pentru asta)
        private async void BtnDeleteTrivia_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int id = (int)btn.Tag;
            var response = await _httpClient.DeleteAsync($"api/trivia/delete/{id}");
            if (response.IsSuccessStatusCode)
            {
                BtnRefreshTrivia_Click(null, null); // Refresh automat
            }
        }

    }

    // Clasă mică pentru a citi răspunsul de la simulare
    public class SimulateResponseDTO
    {
        public string Message { get; set; }
    }
    public class UserDto { public string Username { get; set; } = ""; public decimal Balance { get; set; } public string Role { get; set; } = ""; }
}