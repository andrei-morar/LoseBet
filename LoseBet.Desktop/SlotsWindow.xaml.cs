using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace LoseBet.Desktop
{
    public partial class SlotsWindow : Window
    {
        private string _username;
        private decimal _balance;
        private decimal _castigCurent = 0; // Aici ținem banii care sunt pe "masă" la dublaj

        private string[] _symbols = { "🍒", "🍋", "🍉", "⭐", "💎", "7️⃣" };
        private Random _random = new Random();
        private TextBlock[,] _slotsMatrix;

        public SlotsWindow(string username, string currentBalance)
        {
            InitializeComponent();
            _username = username;
            decimal.TryParse(currentBalance, out _balance);

            _slotsMatrix = new TextBlock[3, 5] {
                { S00, S01, S02, S03, S04 },
                { S10, S11, S12, S13, S14 },
                { S20, S21, S22, S23, S24 }
            };

            UpdateBalanceDisplay();
        }

        private void UpdateBalanceDisplay()
        {
            TxtBalance.Text = $"Sold: {_balance:0.00} RON";
        }

        private async void BtnSpin_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtBetAmount.Text, out decimal betAmount) || betAmount <= 0)
            {
                MessageBox.Show("Miză invalidă!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (betAmount > _balance)
            {
                MessageBox.Show("Fonduri insuficiente!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Scădem miza
            _balance -= betAmount;
            UpdateBalanceDisplay();
            TxtStatus.Text = "Se rotește...";
            TxtStatus.Foreground = Brushes.Black;

            // Ascundem panoul de dublaj dacă era deschis
            PanelDublaj.Visibility = Visibility.Collapsed;

            // Efect de rotire
            for (int i = 0; i < 12; i++)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        _slotsMatrix[row, col].Text = _symbols[_random.Next(_symbols.Length)];
                    }
                }
                await Task.Delay(50);
            }

            decimal totalWin = 0;
            for (int row = 0; row < 3; row++)
            {
                if (_slotsMatrix[row, 0].Text == _slotsMatrix[row, 1].Text &&
                    _slotsMatrix[row, 1].Text == _slotsMatrix[row, 2].Text)
                {
                    if (_slotsMatrix[row, 2].Text == _slotsMatrix[row, 3].Text &&
                        _slotsMatrix[row, 3].Text == _slotsMatrix[row, 4].Text)
                        totalWin += betAmount * 25; // 5 la rând
                    else if (_slotsMatrix[row, 2].Text == _slotsMatrix[row, 3].Text)
                        totalWin += betAmount * 10; // 4 la rând
                    else
                        totalWin += betAmount * 3;  // 3 la rând
                }
            }

            if (totalWin > 0)
            {
                // AICI E MAGIA: NU adăugăm în sold încă! Îi punem pe "masă".
                _castigCurent = totalWin;
                TxtStatus.Text = $"AI CÂȘTIGAT! {totalWin} RON. Alegi dublaj?";
                TxtStatus.Foreground = Brushes.Green;

                // Afișăm panoul de dublaj și blocăm butonul de SPIN până se decide
                TxtDublajAmount.Text = $"Suma pe masă: {_castigCurent} RON";
                PanelDublaj.Visibility = Visibility.Visible;
                // Dezactivăm temporar butonul de rotire ca să nu fenteze sistemul
                if (sender is Button btnSpin) btnSpin.IsEnabled = false;
            }
            else
            {
                TxtStatus.Text = "Ai pierdut. Mai încearcă!";
                TxtStatus.Foreground = Brushes.Red;
            }
        }

        // ==========================================
        // FUNCȚIILE PENTRU DUBLAJ
        // ==========================================

        private void BtnRosie_Click(object sender, RoutedEventArgs e)
        {
            JoacaDublaj(true); // true = a pariat pe roșie
        }

        private void BtnNeagra_Click(object sender, RoutedEventArgs e)
        {
            JoacaDublaj(false); // false = a pariat pe neagră
        }

        private void JoacaDublaj(bool pariatPeRosu)
        {
            // Tragem o carte random: 0 = Roșie, 1 = Neagră
            int carteRandom = _random.Next(0, 2);
            bool carteaERosie = (carteRandom == 0);
            string numeCarte = carteaERosie ? "🔴 ROȘIE" : "⚫ NEAGRĂ";

            if (pariatPeRosu == carteaERosie)
            {
                // AI GHICIT! Dublăm banii
                _castigCurent *= 2;
                TxtDublajAmount.Text = $"Suma pe masă: {_castigCurent} RON";
                TxtStatus.Text = $"BRAVO! Cartea a fost {numeCarte}. Ai dublat!";
                TxtStatus.Foreground = Brushes.Green;
            }
            else
            {
                // AI PIERDUT! Pierzi tot ce era pe masă
                _castigCurent = 0;
                PanelDublaj.Visibility = Visibility.Collapsed;

                TxtStatus.Text = $"GHINION! Cartea a fost {numeCarte}. Ai pierdut câștigul.";
                TxtStatus.Foreground = Brushes.Red;

                // Reactivăm butonul de rotire
                ReactivareSpinButton();
            }
        }

        private void BtnIncaseaza_Click(object sender, RoutedEventArgs e)
        {
            // Adaugă banii în soldul real
            _balance += _castigCurent;
            UpdateBalanceDisplay();

            _castigCurent = 0;
            PanelDublaj.Visibility = Visibility.Collapsed;

            TxtStatus.Text = "Câștig încasat! Poți roti din nou.";
            TxtStatus.Foreground = Brushes.Blue;

            ReactivareSpinButton();
        }

        private void ReactivareSpinButton()
        {
            // Căutăm butonul de spin în Grid și îl reactivăm
            foreach (var element in ((StackPanel)PanelDublaj.Parent).Parent is Grid parentGrid ? parentGrid.Children : null)
            {
                if (element is StackPanel sp)
                {
                    foreach (var child in sp.Children)
                    {
                        if (child is Button b && (string)b.Content == "🎰 ROTIRE (SPIN)")
                        {
                            b.IsEnabled = true;
                        }
                    }
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            SlotsLobbyWindow lobby = new SlotsLobbyWindow(_username, _balance.ToString());
            lobby.Show();
            this.Close();
        }
    }
}