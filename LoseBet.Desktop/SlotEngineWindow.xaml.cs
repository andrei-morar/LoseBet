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
        private decimal[] _mize = { 0.20m, 0.40m, 0.80m, 1.20m, 2.00m, 5.00m, 10.00m, 20.00m, 50.00m, 100.00m, 200.00m };
        private int _betIndex = 0;
        private decimal _lastWinAmount = 0;
        private List<Image> _cells = new List<Image>();
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        public SlotEngineWindow(SlotGame game)
        {
            InitializeComponent();
            _game = game;
            TxtGameName.Text = game.Name.ToUpper();
            TxtCurrentBet.Text = _mize[_betIndex].ToString("0.00");

            RefreshBalance();
            SetupGrid();
        }

        private async void RefreshBalance()
        {
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"{balance:F2} RON";
            GameService.NotifyBalanceChanged();
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
            TxtLastWin.Visibility = Visibility.Hidden;
            BtnGamble.Visibility = Visibility.Collapsed;

            decimal miza = _mize[_betIndex];

            decimal balantaCurenta = await GameService.GetBalanceAsync();
            if (miza > balantaCurenta)
            {
                MessageBox.Show("Fonduri insuficiente!");
                BtnSpin.IsEnabled = true;
                return;
            }

            try
            {
                var payload = new { UserId = UserSession.UserId, GameId = _game.Id, BetAmount = miza };
                var resp = await _httpClient.PostAsJsonAsync("api/slots/spin", payload);

                if (resp.IsSuccessStatusCode)
                {
                    var res = await resp.Content.ReadFromJsonAsync<SlotSpinResponseDTO>();

                    // Apelăm animația de Spin
                    await AnimateSpinAsync(res);

                    RefreshBalance(); // Banii se actualizează la finalul animației

                    if (res.TotalWin > 0)
                    {
                        _lastWinAmount = res.TotalWin; // Am adăugat salvarea sumei aici!
                        TxtLastWin.Text = $"WIN: {res.TotalWin:F2}";
                        TxtLastWin.Visibility = Visibility.Visible;
                        BtnGamble.Visibility = Visibility.Visible;
                    }
                }
            }
            catch { MessageBox.Show("Eroare de conexiune la Spin."); }

            BtnSpin.IsEnabled = true;
        }

        // METODA PENTRU ANIMAȚIE
        private async Task AnimateSpinAsync(SlotSpinResponseDTO finalResult)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Slots");
            Random rnd = new Random();

            // Extragem toate pozele din acest joc pentru a face blur/spin fake
            List<string> allImages = new List<string>();
            foreach (var sym in _game.Symbols) allImages.Add(sym.ImageName);
            if (allImages.Count == 0) return;

            // 1. Efectul de "vârtej" - Schimbăm pozele rapid de 10 ori
            int spinFrames = 10;
            for (int frame = 0; frame < spinFrames; frame++)
            {
                for (int i = 0; i < _cells.Count; i++)
                {
                    string randomImg = allImages[rnd.Next(allImages.Count)];
                    string p = Path.Combine(dir, randomImg);
                    if (File.Exists(p)) _cells[i].Source = new BitmapImage(new Uri(p));
                }
                await Task.Delay(40); // Viteza de rotire
            }

            // 2. Oprirea pe coloane (de la stânga la dreapta)
            for (int c = 0; c < _game.Columns; c++)
            {
                for (int r = 0; r < _game.Rows; r++)
                {
                    string finalImg = finalResult.Grid[r][c];
                    string p = Path.Combine(dir, finalImg);
                    if (File.Exists(p)) _cells[r * _game.Columns + c].Source = new BitmapImage(new Uri(p));
                }

                // Pauză ca să se audă/vadă cum cade fiecare rolă
                await Task.Delay(150);
            }
        }

        // METODA NOUĂ PENTRU DUBLAJ
        private void BtnGamble_Click(object sender, RoutedEventArgs e)
        {
            // Ascundem butonul ca să nu poată da de două ori
            BtnGamble.Visibility = Visibility.Collapsed;

            // Deschidem fereastra, trimițându-i suma pe care o riscă
            GambleWindow gw = new GambleWindow(_lastWinAmount);
            gw.Owner = this;
            gw.ShowDialog();

            // După ce se închide fereastra (fie că a pierdut, fie că a luat banii)
            RefreshBalance();
            TxtLastWin.Visibility = Visibility.Hidden;
        }

        private void BtnLowerBet_Click(object sender, RoutedEventArgs e) { if (_betIndex > 0) _betIndex--; TxtCurrentBet.Text = _mize[_betIndex].ToString("0.00"); }
        private void BtnHigherBet_Click(object sender, RoutedEventArgs e) { if (_betIndex < _mize.Length - 1) _betIndex++; TxtCurrentBet.Text = _mize[_betIndex].ToString("0.00"); }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            SlotsLobbyWindow lobby = new SlotsLobbyWindow();
            lobby.Show();
            this.Close();
        }
    }

    // Clasa auxiliară pentru a citi răspunsul de la Spin
    public class SlotSpinResponseDTO
    {
        public string[][] Grid { get; set; }
        public decimal NewBalance { get; set; }
        public decimal TotalWin { get; set; }
        public bool IsWin { get; set; }
        public string Message { get; set; }
    }
}