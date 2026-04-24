using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoseBet.Desktop
{
    public partial class BlackjackGameWindow : Window
    {
        private decimal _balance;
        private string _username;
        private decimal _currentBet;

        private List<string> _deck;
        private List<string> _playerCards;
        private List<string> _dealerCards;
        private Random _random = new Random();

        public BlackjackGameWindow(string username, string balance)
        {
            InitializeComponent();
            _username = username;
            decimal.TryParse(balance, out _balance);
            UpdateBalanceDisplay();
        }

        private void UpdateBalanceDisplay() => TxtBalance.Text = $"Sold: {_balance:0.00} RON";

        // Creăm și amestecăm pachetul de cărți
        private void InitializeDeck()
        {
            _deck = new List<string>();
            string[] suits = { "♥", "♦", "♣", "♠" };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (var suit in suits)
            {
                foreach (var val in values)
                {
                    _deck.Add($"{val}{suit}");
                }
            }
            _deck = _deck.OrderBy(x => _random.Next()).ToList();
        }

        private string DrawCard()
        {
            string card = _deck[0];
            _deck.RemoveAt(0);
            return card;
        }

        // Calculam scorul (Așii sunt 11 sau 1)
        private int CalculateScore(List<string> hand)
        {
            int score = 0;
            int aces = 0;

            foreach (string card in hand)
            {
                string value = card.Substring(0, card.Length - 1);
                if (value == "J" || value == "Q" || value == "K") score += 10;
                else if (value == "A") { score += 11; aces++; }
                else score += int.Parse(value);
            }

            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }

            return score;
        }

        // Desenăm cartea pe ecran vizual
        private UIElement CreateCardVisual(string cardStr, bool hidden = false)
        {
            Border b = new Border
            {
                Width = 80,
                Height = 110,
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(5)
            };

            if (hidden)
            {
                b.Background = Brushes.DarkRed; // Cartea întoarsă a dealerului
            }
            else
            {
                bool isRed = cardStr.Contains("♥") || cardStr.Contains("♦");
                TextBlock txt = new TextBlock
                {
                    Text = cardStr,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = isRed ? Brushes.Red : Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                b.Child = txt;
            }
            return b;
        }

        private void UpdateUI(bool hideDealerSecondCard = true)
        {
            PanelPlayerCards.Children.Clear();
            foreach (var card in _playerCards) PanelPlayerCards.Children.Add(CreateCardVisual(card));
            TxtPlayerScore.Text = $"Scorul tău: {CalculateScore(_playerCards)}";

            PanelDealerCards.Children.Clear();
            for (int i = 0; i < _dealerCards.Count; i++)
            {
                if (i == 1 && hideDealerSecondCard) PanelDealerCards.Children.Add(CreateCardVisual(_dealerCards[i], true));
                else PanelDealerCards.Children.Add(CreateCardVisual(_dealerCards[i]));
            }

            if (hideDealerSecondCard) TxtDealerScore.Text = "Cărțile Dealerului: ?";
            else TxtDealerScore.Text = $"Cărțile Dealerului: {CalculateScore(_dealerCards)}";
        }

        private void BtnDeal_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtBetAmount.Text, out _currentBet) || _currentBet <= 0 || _currentBet > _balance)
            {
                MessageBox.Show("Miza invalidă sau fonduri insuficiente!"); return;
            }

            _balance -= _currentBet;
            UpdateBalanceDisplay();

            InitializeDeck();
            _playerCards = new List<string> { DrawCard(), DrawCard() };
            _dealerCards = new List<string> { DrawCard(), DrawCard() };

            PanelBetting.Visibility = Visibility.Collapsed;
            PanelActions.Visibility = Visibility.Visible;
            TxtStatus.Text = "Hit (Trage) sau Stand (Stai)?";
            TxtStatus.Foreground = Brushes.White;

            UpdateUI(true);

            if (CalculateScore(_playerCards) == 21) EndGame(); // Ai prins Blackjack direct!
        }

        private void BtnHit_Click(object sender, RoutedEventArgs e)
        {
            _playerCards.Add(DrawCard());
            UpdateUI(true);

            if (CalculateScore(_playerCards) > 21) EndGame(); // Ai sărit de 21
        }

        private async void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;

            UpdateUI(false); // Afișăm cartea ascunsă a dealerului

            // Dealerul trebuie să tragă până face minim 17
            while (CalculateScore(_dealerCards) < 17)
            {
                await Task.Delay(1000);
                _dealerCards.Add(DrawCard());
                UpdateUI(false);
            }

            EndGame();
        }

        private void EndGame()
        {
            BtnHit.IsEnabled = true;
            BtnStand.IsEnabled = true;
            UpdateUI(false);

            int pScore = CalculateScore(_playerCards);
            int dScore = CalculateScore(_dealerCards);

            if (pScore > 21)
            {
                TxtStatus.Text = "BUST! Ai depășit 21. Ai pierdut miza.";
                TxtStatus.Foreground = Brushes.Tomato;
            }
            else if (dScore > 21 || pScore > dScore)
            {
                decimal win = _currentBet * 2;
                if (pScore == 21 && _playerCards.Count == 2) win = _currentBet * 2.5m; // Blackjack-ul plătește mai mult!
                _balance += win;
                TxtStatus.Text = $"CÂȘTIG! Primești {win} RON.";
                TxtStatus.Foreground = Brushes.LightGreen;
            }
            else if (pScore == dScore)
            {
                _balance += _currentBet; // Egalitate, primești miza înapoi
                TxtStatus.Text = "EGALITATE (PUSH). Miza returnată.";
                TxtStatus.Foreground = Brushes.Yellow;
            }
            else
            {
                TxtStatus.Text = "DEALERUL CÂȘTIGĂ!";
                TxtStatus.Foreground = Brushes.Tomato;
            }

            UpdateBalanceDisplay();
            PanelBetting.Visibility = Visibility.Visible;
            PanelActions.Visibility = Visibility.Collapsed;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Ne întoarcem în Lobby
            BlackJackLobbyWindow lobby = new BlackJackLobbyWindow(_username, _balance.ToString());
            lobby.Show();
            this.Close();
        }
    }
}