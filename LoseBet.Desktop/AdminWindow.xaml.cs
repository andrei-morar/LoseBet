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

        private List<SymbolConfigDTO> _configuredSymbols = new List<SymbolConfigDTO>();
        private int? _selectedSlotId = null;

        public AdminWindow(string adminUsername)
        {
            InitializeComponent();
            _adminUsername = adminUsername;
            _ = LoadUsersAsync();
            _ = LoadSlotsAsync();
        }

        // ==========================================
        // TAB 1: USERS
        // ==========================================
        private async Task LoadUsersAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync($"api/admin/users?adminUsername={_adminUsername}");
                if (res.IsSuccessStatusCode)
                    UsersGrid.ItemsSource = await res.Content.ReadFromJsonAsync<List<UserDto>>();
            }
            catch { /* Silent Catch */ }
        }

        private async void BtnToggleBan_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserDto u)
            {
                try
                {
                    await _httpClient.PostAsJsonAsync("api/admin/toggle-ban", new { AdminUsername = _adminUsername, TargetUsername = u.Username });
                    await LoadUsersAsync();
                }
                catch { MessageBox.Show("Eroare de conexiune la server."); }
            }
        }

        private async void BtnUpdateRole_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserDto u && CmbRoles.SelectedItem is ComboBoxItem i)
            {
                try
                {
                    await _httpClient.PostAsJsonAsync("api/admin/update-role", new { AdminUsername = _adminUsername, TargetUsername = u.Username, NewRole = i.Content.ToString() });
                    await LoadUsersAsync();
                }
                catch { MessageBox.Show("Eroare de conexiune la server."); }
            }
        }

        private async void BtnUpdateMoney_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserDto u && decimal.TryParse(TxtAmount.Text, out decimal amt))
            {
                try
                {
                    await _httpClient.PostAsJsonAsync("api/admin/update-balance", new { AdminUsername = _adminUsername, TargetUsername = u.Username, NewBalance = amt });
                    await LoadUsersAsync();
                    TxtAmount.Clear();
                }
                catch { MessageBox.Show("Eroare de conexiune la server."); }
            }
            else
            {
                MessageBox.Show("Introdu o sumă validă!");
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => _ = LoadUsersAsync();

        // ==========================================
        // TAB 2: SLOT MANAGER
        // ==========================================
        private async Task LoadSlotsAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync("api/slots/active");
                if (res.IsSuccessStatusCode)
                {
                    SlotsGrid.ItemsSource = await res.Content.ReadFromJsonAsync<List<SlotGame>>();
                }
            }
            catch { /* Silent Catch */ }
        }

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

                _configuredSymbols.Clear();

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

                LstConfiguredSymbols.ItemsSource = null;
                LstConfiguredSymbols.ItemsSource = _configuredSymbols;

                BtnUploadSymbols.Content = $"📁 Adăugă Simbol Nou ({_configuredSymbols.Count})";
                BtnUploadSymbols.Background = _configuredSymbols.Count > 0
                    ? System.Windows.Media.Brushes.MediumSeaGreen
                    : new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2A2B35"));
            }
        }

        private async void BtnDeleteSlot_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSlotId != null)
            {
                try
                {
                    await _httpClient.DeleteAsync($"api/slots/delete/{_selectedSlotId}");
                    await LoadSlotsAsync();
                    BtnNewMode_Click(null, null);
                }
                catch { MessageBox.Show("Eroare de conexiune."); }
            }
        }

        private void BtnNewMode_Click(object? sender, RoutedEventArgs? e)
        {
            _selectedSlotId = null;
            TxtSlotName.Text = "";
            TxtSlotThumb.Text = "Selectează o poză...";
            TxtSlotColor.Text = "#1E1E1E";

            _configuredSymbols.Clear();
            LstConfiguredSymbols.ItemsSource = null;

            BtnUploadSymbols.Content = "➕ Adaugă Simbol Nou";
            BtnUploadSymbols.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2A2B35"));
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

                LstConfiguredSymbols.ItemsSource = null;
                LstConfiguredSymbols.ItemsSource = _configuredSymbols;

                BtnUploadSymbols.Content = _configuredSymbols.Count > 0
                    ? $"📁 Adăugă Simbol Nou ({_configuredSymbols.Count})"
                    : "➕ Adaugă Simbol Nou";

                if (_configuredSymbols.Count == 0)
                {
                    BtnUploadSymbols.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2A2B35"));
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

            try
            {
                var data = new
                {
                    Name = TxtSlotName.Text,
                    ThumbnailUrl = TxtSlotThumb.Text,
                    ThemeColor = TxtSlotColor.Text,
                    Rows = int.Parse(TxtSlotRows.Text),
                    Columns = int.Parse(TxtSlotCols.Text),
                    Paylines = int.Parse(TxtSlotPaylines.Text),
                    WildType = ((ComboBoxItem)CmbWildType.SelectedItem).Content.ToString(),
                    Symbols = _configuredSymbols
                };

                if (_selectedSlotId == null)
                    await _httpClient.PostAsJsonAsync("api/slots/add", data);
                else
                    await _httpClient.PutAsJsonAsync($"api/slots/update/{_selectedSlotId}", data);

                await LoadSlotsAsync();
                BtnNewMode_Click(null, null);
                BtnUploadSymbols.Content = "📁 Încarcă Simboluri (Multi)";
                BtnUploadSymbols.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2A2B35"));

                MessageBox.Show("Jocul a fost salvat cu succes în baza de date!", "Succes");
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare la salvarea slotului: " + ex.Message);
            }
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
        // TAB 3: PARIURI SPORTIVE
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
            catch { /* Silent Catch */ }
        }

        private async void BtnAddMatch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHomeTeam.Text) || string.IsNullOrWhiteSpace(TxtAwayTeam.Text))
            {
                MessageBox.Show("Completează numele ambelor echipe!");
                return;
            }

            try
            {
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
                    MessageBox.Show("Meciul a fost adăugat în oferta casei de pariuri!", "Succes");
                    TxtHomeTeam.Text = "";
                    TxtAwayTeam.Text = "";
                    TxtOdds1.Text = "1.00";
                    TxtOddsX.Text = "1.00";
                    TxtOdds2.Text = "1.00";
                    await LoadMatchesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare adăugare meci: " + ex.Message);
            }
        }

        private void BtnRefreshMatches_Click(object sender, RoutedEventArgs e) => _ = LoadMatchesAsync();

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
                            var responseData = await res.Content.ReadFromJsonAsync<SimulateResponseDTO>();
                            MessageBox.Show(responseData?.Message, "Fluier final!", MessageBoxButton.OK, MessageBoxImage.Information);
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

        // ==========================================
        // TAB 4: TRIVIA
        // ==========================================
        private void CmbTriviaType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelTriviaOptions == null) return;

            var selected = (ComboBoxItem)CmbTriviaType.SelectedItem;
            if (selected.Tag?.ToString() == "1") // Aproximare
            {
                PanelTriviaOptions.Visibility = Visibility.Collapsed;
            }
            else // Grila
            {
                PanelTriviaOptions.Visibility = Visibility.Visible;
            }
        }

        private async void BtnAddTrivia_Click(object sender, RoutedEventArgs e)
        {
            var selectedType = (ComboBoxItem)CmbTriviaType.SelectedItem;
            int type = int.Parse(selectedType.Tag?.ToString() ?? "2");

            var newQuestion = new LoseBet.Core.Models.TriviaQuestion
            {
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

        private async void BtnRefreshTrivia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var questions = await _httpClient.GetFromJsonAsync<List<LoseBet.Core.Models.TriviaQuestion>>("api/trivia/questions");
                TriviaGrid.ItemsSource = questions;
            }
            catch { MessageBox.Show("Eroare la încărcarea întrebărilor."); }
        }

        private async void BtnDeleteTrivia_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int id = (int)btn.Tag;
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/trivia/delete/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        BtnRefreshTrivia_Click(null!, null!);
                    }
                }
                catch { MessageBox.Show("Eroare la ștergere."); }
            }
        }
    }

    public class SimulateResponseDTO
    {
        public string? Message { get; set; }
    }

    public class UserDto
    {
        public string Username { get; set; } = "";
        public decimal Balance { get; set; }
        public string Role { get; set; } = "";
    }
}