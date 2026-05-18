using System;
using System.Windows;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class GambleWindow : Window
    {
        public decimal ResultAmount { get; private set; }
        private decimal _currentAmount;
        private Random _random = new Random();

        public GambleWindow(decimal amount)
        {
            InitializeComponent();
            _currentAmount = amount;
            TxtGambleAmount.Text = _currentAmount.ToString("0.00");
            ResultAmount = 0; // Dacă închide fereastra fără să încaseze, pierde tot (opțional)
        }

        private void BtnRed_Click(object sender, RoutedEventArgs e) => Play(true);
        private void BtnBlack_Click(object sender, RoutedEventArgs e) => Play(false);

        private void Play(bool choseRed)
        {
            // 50-50 șanse
            bool isRed = _random.Next(0, 2) == 0;

            // Vizual: arătăm cartea
            CardBorder.Background = isRed ? Brushes.Red : Brushes.Black;
            TxtCardSymbol.Text = isRed ? "♥" : "♣";
            TxtCardSymbol.Foreground = Brushes.White;

            if (choseRed == isRed)
            {
                // A câștigat: Dublăm!
                _currentAmount *= 2;
                TxtGambleAmount.Text = _currentAmount.ToString("0.00");

                if (_currentAmount >= 5000) // Limită de siguranță
                {
                    MessageBox.Show("Limită de dublaj atinsă!");
                    BtnCollect_Click(null, null);
                }
            }
            else
            {
                // A pierdut tot
                MessageBox.Show("Ai pierdut!");
                _currentAmount = 0;
                ResultAmount = 0;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnCollect_Click(object sender, RoutedEventArgs e)
        {
            ResultAmount = _currentAmount;
            this.DialogResult = true;
            this.Close();
        }
    }
}