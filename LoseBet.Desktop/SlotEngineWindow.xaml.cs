using LoseBet.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace LoseBet.Desktop
{
    public partial class SlotEngineWindow : Window
    {
        private SlotGame _game;
        private string _username;
        private decimal[] _mize = { 0.20m, 0.40m, 0.80m, 1.20m, 2.00m, 5.00m, 10.00m, 20.00m, 50.00m, 100.00m, 200.00m };
        private int _betIndex = 0;
        private List<Image> _cells = new List<Image>();
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        public SlotEngineWindow(string username, string balance, SlotGame game)
        {
            InitializeComponent();
            _username = username;
            _game = game;
            TxtGameName.Text = game.Name.ToUpper();
            TxtBalance.Text = $"{balance} RON";
            TxtCurrentBet.Text = _mize[_betIndex].ToString("0.00");
            SetupGrid();
        }

        private void SetupGrid()
        {
            SlotGrid.Rows = _game.Rows;
            SlotGrid.Columns = _game.Columns;
            SlotGrid.Children.Clear();
            _cells.Clear();
            for (int i = 0; i < _game.Rows * _game.Columns; i++)
            {
                Image img = new Image { Stretch = Stretch.Uniform, Margin = new Thickness(5) };
                SlotGrid.Children.Add(new Border { Child = img, BorderThickness = new Thickness(0.5), BorderBrush = Brushes.Gray });
                _cells.Add(img);
            }
        }

        private async void BtnSpin_Click(object sender, RoutedEventArgs e)
        {
            BtnSpin.IsEnabled = false;
            try
            {
                var resp = await _httpClient.PostAsJsonAsync("api/slots/spin", new { Username = _username, GameId = _game.Id, BetAmount = _mize[_betIndex] });
                if (resp.IsSuccessStatusCode)
                {
                    var res = await resp.Content.ReadFromJsonAsync<SlotSpinResponse>();
                    string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Slots");
                    for (int r = 0; r < _game.Rows; r++)
                        for (int c = 0; c < _game.Columns; c++)
                        {
                            string p = Path.Combine(dir, res.Grid[r][c]);
                            if (File.Exists(p)) _cells[r * _game.Columns + c].Source = new BitmapImage(new Uri(p));
                        }
                    TxtBalance.Text = $"{res.NewBalance:0.00} RON";
                    if (res.TotalWin > 0) { TxtLastWin.Text = $"WIN: {res.TotalWin}"; TxtLastWin.Visibility = Visibility.Visible; BtnGamble.Visibility = Visibility.Visible; }
                }
            }
            catch { }
            BtnSpin.IsEnabled = true;
        }

        // METODELE PENTRU BUTOANELE DIN XAML (REPARĂ CS1061)
        private void BtnGamble_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Dublaj!");
        private void BtnLowerBet_Click(object sender, RoutedEventArgs e) { if (_betIndex > 0) _betIndex--; TxtCurrentBet.Text = _mize[_betIndex].ToString("0.00"); }
        private void BtnHigherBet_Click(object sender, RoutedEventArgs e) { if (_betIndex < _mize.Length - 1) _betIndex++; TxtCurrentBet.Text = _mize[_betIndex].ToString("0.00"); }
        private void BtnExit_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}