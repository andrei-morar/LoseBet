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

namespace LoseBet.Desktop
{
    public partial class AdminWindow : Window
    {
        private readonly string _adminUsername;
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };
        private List<string> _symbolFiles = new List<string>();
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

        // --- EVENIMENTE XAML ---
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

        private void SlotsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid?.SelectedItem is SlotGame s)
            {
                _selectedSlotId = s.Id;
                TxtSlotName.Text = s.Name;
                TxtSlotThumb.Text = s.ThumbnailUrl;
            }
        }

        private async void BtnDeleteSlot_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSlotId != null)
            {
                await _httpClient.DeleteAsync($"api/slots/delete/{_selectedSlotId}");
                await LoadSlotsAsync();
                BtnNewMode_Click(null, null); // Resetăm formularul după ștergere
            }
        }

        // METODA CARE LIPSEA PENTRU "MOD NOU"
        private void BtnNewMode_Click(object sender, RoutedEventArgs e)
        {
            _selectedSlotId = null;
            TxtSlotName.Text = "";
            TxtSlotThumb.Text = "";
            _symbolFiles.Clear();
        }

        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Imagini (*.png;*.jpg)|*.png;*.jpg" };
            if (op.ShowDialog() == true) TxtSlotThumb.Text = CopyToResources(op.FileName);
        }

        private void BtnUploadSymbols_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Multiselect = true, Filter = "Imagini (*.png;*.jpg)|*.png;*.jpg" };
            if (op.ShowDialog() == true)
            {
                foreach (var f in op.FileNames) _symbolFiles.Add(CopyToResources(f));
                MessageBox.Show($"Au fost adăugate {_symbolFiles.Count} simboluri noi.");
            }
        }

        private async void BtnAddSlot_Click(object sender, RoutedEventArgs e)
        {
            var data = new { Name = TxtSlotName.Text, ThumbnailUrl = TxtSlotThumb.Text, Rows = int.Parse(TxtSlotRows.Text), Columns = int.Parse(TxtSlotCols.Text), SymbolImages = _symbolFiles };
            if (_selectedSlotId == null) await _httpClient.PostAsJsonAsync("api/slots/add", data);
            else await _httpClient.PutAsJsonAsync($"api/slots/update/{_selectedSlotId}", data);

            await LoadSlotsAsync();
            BtnNewMode_Click(null, null); // Resetăm formularul după salvare
        }

        private string CopyToResources(string src)
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Slots");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string name = Path.GetFileName(src);
            File.Copy(src, Path.Combine(folder, name), true);
            return name;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => _ = LoadUsersAsync();
    }

    public class UserDto { public string Username { get; set; } = ""; public decimal Balance { get; set; } public string Role { get; set; } = ""; }
}