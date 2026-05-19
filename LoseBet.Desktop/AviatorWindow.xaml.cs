using LoseBet.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LoseBet.Desktop
{
    public partial class AviatorWindow : Window
    {
        private DispatcherTimer _gameTimer;
        private DispatcherTimer _waitTimer;

        private decimal _currentMultiplier = 1.00m;
        private decimal _crashPoint;
        private int _currentSessionId;

        // Stările jocului
        private enum GameState { Waiting, Flying, Crashed }
        private GameState _currentState = GameState.Crashed; // Începem prăbușiți ca să poată pune primul pariu

        private bool _hasBetForNextRound = false;
        private decimal _stagedBetAmount = 0;

        // VARIABILE PENTRU AUTO CASHOUT
        private bool _activeAutoCashout = false;
        private decimal _activeTargetMultiplier = 0;

        // Istoric (observabil pentru a actualiza UI-ul)
        private ObservableCollection<HistoryItem> _history = new ObservableCollection<HistoryItem>();

        // Animație Canvas
        private double _timeElapsed = 0;

        public AviatorWindow()
        {
            InitializeComponent();

            HistoryPanel.ItemsSource = _history;

            // Timer pentru zbor (Foarte rapid pentru animație lină)
            _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _gameTimer.Tick += GameTimer_Tick;

            // Timer pentru bara de așteptare
            _waitTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _waitTimer.Tick += WaitTimer_Tick;

            RefreshBalance();

            // Simulăm un istoric inițial fals, ca să arate bine
            AddFakeHistory();

            // Pornim ciclul jocului
            StartWaitPhase();
        }

        private async void RefreshBalance()
        {
            decimal balance = await GameService.GetBalanceAsync();
            TxtBalance.Text = $"{balance:F2} RON";
            GameService.NotifyBalanceChanged();
        }

        // =====================================
        // CICLUL JOCULUI
        // =====================================

        private void StartWaitPhase()
        {
            _currentState = GameState.Waiting;

            MultiplierDisplay.Text = "1.00x";
            MultiplierDisplay.Foreground = Brushes.White;
            StatusText.Text = "AȘTEPTARE PARIURI";
            StatusText.Foreground = Brushes.Orange;
            WaitProgressBar.Visibility = Visibility.Visible;
            WaitProgressBar.Value = 100;

            ResetCanvas();

            // Dacă omul apucase să dea "Pariază" la tura trecută (pentru runda viitoare), acum are un pariu valid.
            if (_hasBetForNextRound)
            {
                SetBtnStateCashoutReady(); // E gata de cashout
            }
            else
            {
                SetBtnStateBet(); // Poate paria
            }

            _waitTimer.Start();
        }

        private void WaitTimer_Tick(object sender, EventArgs e)
        {
            WaitProgressBar.Value -= 1.5; // Scade bara de progres

            if (WaitProgressBar.Value <= 0)
            {
                _waitTimer.Stop();
                StartFlightPhase();
            }
        }

        private async void StartFlightPhase()
        {
            _currentState = GameState.Flying;
            WaitProgressBar.Visibility = Visibility.Hidden;
            StatusText.Text = "";
            MultiplierDisplay.Foreground = Brushes.White;

            // Generăm un crash fictiv local (Atenție: ideal asta vine server-side, dar o simulăm pentru cursivitate)
            // Dacă a pariat, API-ul tău sigur i-a dat un CrashPoint în _crashPoint. Dacă nu, generăm unul.
            if (!_hasBetForNextRound)
            {
                _crashPoint = GenerateLocalCrashPoint();
            }

            _currentMultiplier = 1.00m;
            _timeElapsed = 0;

            // Dacă a pariat, butonul devine portocaliu (Cashout)
            if (_hasBetForNextRound)
            {
                SetBtnStateCashout();
            }
            else
            {
                SetBtnStateWaitingNext(); // "Pariază pentru runda următoare"
            }

            _gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Creșterea exponențială a multiplicatorului
            _currentMultiplier += 0.003m + (_currentMultiplier * 0.007m);
            _timeElapsed += 0.03;

            // Desenăm pe Canvas
            AnimatePlane(_timeElapsed, (double)_currentMultiplier);

            // ===============================================
            // VERIFICARE AUTO CASHOUT (Adăugat)
            // ===============================================
            if (_hasBetForNextRound && _currentState == GameState.Flying && _activeAutoCashout)
            {
                if (_currentMultiplier >= _activeTargetMultiplier)
                {
                    // Trigem cashout automat la valoarea țintă
                    _ = ExecuteCashout(_activeTargetMultiplier);
                }
            }

            // Verificăm prăbușirea
            if (_currentMultiplier >= _crashPoint)
            {
                Crash();
            }
            else
            {
                MultiplierDisplay.Text = $"{_currentMultiplier:F2}x";

                // Actualizăm textul de pe butonul de Cashout cu profitul potențial
                if (_hasBetForNextRound && _currentState == GameState.Flying)
                {
                    BtnBetSubText.Text = $"Câștig: {(_stagedBetAmount * _currentMultiplier):F2} RON";
                }
            }
        }

        private void Crash()
        {
            _gameTimer.Stop();
            _currentState = GameState.Crashed;
            _hasBetForNextRound = false; // Biletul s-a dus

            MultiplierDisplay.Text = $"{_crashPoint:F2}x";
            MultiplierDisplay.Foreground = Brushes.Red;
            StatusText.Text = "Flew Away!";
            StatusText.Foreground = Brushes.Red;

            PlaneIcon.Foreground = Brushes.Gray; // Avionul se face gri
            FlightPath.Stroke = Brushes.Gray;

            // Adăugăm în istoric
            AddToHistory(_crashPoint);

            SetBtnStateBet(); // Se resetează pentru runda nouă

            // Așteptăm 3 secunde și o luăm de la capăt
            Task.Delay(3000).ContinueWith(t => Dispatcher.Invoke(StartWaitPhase));
        }

        // =====================================
        // PARIERE ȘI CASHOUT
        // =====================================

        private async void BtnBet_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == GameState.Waiting && !_hasBetForNextRound)
            {
                // Pariază pentru runda care stă să înceapă
                if (decimal.TryParse(BetInput.Text, out decimal betAmount) && betAmount > 0)
                {
                    var result = await GameService.StartAviatorAsync(betAmount);
                    if (result != null)
                    {
                        _currentSessionId = result.SessionId;
                        _crashPoint = result.CrashPoint;
                        _stagedBetAmount = betAmount;
                        _hasBetForNextRound = true;

                        // ===============================================
                        // SALVĂM SETĂRILE DE AUTO CASHOUT (Adăugat)
                        // ===============================================
                        if (ChkAutoCashout.IsChecked == true && decimal.TryParse(AutoCashoutInput.Text, out decimal target) && target > 1.00m)
                        {
                            _activeAutoCashout = true;
                            _activeTargetMultiplier = target;
                        }
                        else
                        {
                            _activeAutoCashout = false;
                        }

                        RefreshBalance();
                        SetBtnStateCashoutReady();
                    }
                    else { MessageBox.Show("Fonduri insuficiente!"); }
                }
            }
            else if (_currentState == GameState.Flying && !_hasBetForNextRound)
            {
                // Pariază în avans pentru runda URMATOARE (având în vedere că avionul e în zbor)
                // (Omitere complexă: Pentru simplitate, momentan lăsăm doar așteptarea)
            }
            else if (_currentState == GameState.Flying && _hasBetForNextRound)
            {
                // CASHOUT!
                await ExecuteCashout();
            }
        }

        // ===============================================
        // MODIFICAT SĂ PRIMEASCĂ MULTIPLICATOR OPȚIONAL
        // ===============================================
        private async Task ExecuteCashout(decimal? overrideMultiplier = null)
        {
            BtnBet.IsEnabled = false; // Prevent double click

            // Dacă a venit din AutoCashout ia cota fixă, altfel ia cota curentă
            decimal cashoutMultiplier = overrideMultiplier ?? Math.Round(_currentMultiplier, 2);

            var result = await GameService.CashoutAviatorAsync(_currentSessionId, cashoutMultiplier);

            if (result != null && result.Success)
            {
                _hasBetForNextRound = false; // A marcat banii

                // Arătăm un mesaj vizual pe ecran
                StatusText.Text = $"Cashout la {cashoutMultiplier}x !";
                StatusText.Foreground = Brushes.Lime;

                SetBtnStateWaitingNext();
                RefreshBalance();
            }
            else
            {
                BtnBet.IsEnabled = true; // Dacă a dat eroare rețeaua
            }
        }

        // =====================================
        // STĂRILE BUTONULUI PRINCIPAL
        // =====================================

        private void SetBtnStateBet()
        {
            BtnBet.Background = Brushes.MediumSeaGreen;
            BtnBetMainText.Text = "PARIAZĂ";
            BtnBetSubText.Visibility = Visibility.Collapsed;
            BtnBet.IsEnabled = true;
            BetInput.IsEnabled = true;

            // Activăm și căsuțele de AutoCashout ca să le poată schimba între runde
            if (ChkAutoCashout != null) ChkAutoCashout.IsEnabled = true;
            if (AutoCashoutInput != null) AutoCashoutInput.IsEnabled = true;
        }

        private void SetBtnStateCashoutReady()
        {
            BtnBet.Background = Brushes.DarkRed;
            BtnBetMainText.Text = "AȘTEAPTĂ ZBORUL";
            BtnBetSubText.Visibility = Visibility.Collapsed;
            BtnBet.IsEnabled = false;
            BetInput.IsEnabled = false;

            // Blocăm AutoCashout după ce a pariat
            if (ChkAutoCashout != null) ChkAutoCashout.IsEnabled = false;
            if (AutoCashoutInput != null) AutoCashoutInput.IsEnabled = false;
        }

        private void SetBtnStateCashout()
        {
            BtnBet.Background = Brushes.Orange;
            BtnBetMainText.Text = "CASHOUT";
            BtnBetSubText.Visibility = Visibility.Visible;
            BtnBetSubText.Text = $"Câștig: {_stagedBetAmount:F2} RON";
            BtnBet.IsEnabled = true;
        }

        private void SetBtnStateWaitingNext()
        {
            BtnBet.Background = Brushes.DarkGray;
            BtnBetMainText.Text = "AȘTEAPTĂ RUNDA";
            BtnBetSubText.Visibility = Visibility.Collapsed;
            BtnBet.IsEnabled = false;
        }

        // =====================================
        // ANIMAȚIE CANVAS (Grafic exponențial)
        // =====================================

        private void ResetCanvas()
        {
            FlightPath.Data = new PathGeometry();
            FlightPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
            PlaneIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935"));
            Canvas.SetLeft(PlaneIcon, 0);
            Canvas.SetTop(PlaneIcon, FlightCanvas.ActualHeight - 50);
        }

        private void AnimatePlane(double time, double multiplier)
        {
            double width = FlightCanvas.ActualHeight > 0 ? FlightCanvas.ActualWidth : 700;
            double height = FlightCanvas.ActualHeight > 0 ? FlightCanvas.ActualHeight : 350;

            // Calculăm poziția (curba merge în dreapta și urcă exponențial)
            // Limităm X-ul să nu iasă din ecran prea repede
            double x = Math.Min(time * 50, width - 100);

            // Limităm Y-ul. Cu cât e mai mare multiplicatorul, cu atât urcă mai mult (0 e sus, height e jos)
            double curveFactor = Math.Min(multiplier / 5.0, 1.0); // La 5x ajunge sus de tot
            double y = height - 50 - ((height - 100) * Math.Pow(curveFactor, 0.8));

            // Mutăm iconița avionului
            Canvas.SetLeft(PlaneIcon, x);
            Canvas.SetTop(PlaneIcon, y - 20);

            // Construim linia din urmă
            string pathData = $"M 0,{height - 50} Q {x / 2},{height - 50} {x},{y}";
            try { FlightPath.Data = Geometry.Parse(pathData); } catch { }
        }

        // =====================================
        // UTILAJE MINORE
        // =====================================

        private void BtnLowerBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BetInput.Text, out decimal val) && val > 1)
                BetInput.Text = (val - 1).ToString("F2");
        }

        private void BtnHigherBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BetInput.Text, out decimal val))
                BetInput.Text = (val + 1).ToString("F2");
        }

        private void AddToHistory(decimal multiplier)
        {
            if (_history.Count > 10) _history.RemoveAt(_history.Count - 1);

            string color = multiplier < 2 ? "#343a40" : (multiplier < 10 ? "#6f42c1" : "#e83e8c");
            _history.Insert(0, new HistoryItem { Value = multiplier, ColorCode = color });
        }

        private void AddFakeHistory()
        {
            AddToHistory(1.12m); AddToHistory(4.50m); AddToHistory(1.88m); AddToHistory(12.30m); AddToHistory(1.05m);
        }

        private decimal GenerateLocalCrashPoint()
        {
            // Dacă joacă în gol, generăm un multiplicator pe logica clasică de Aviator
            Random rnd = new Random();
            double e = 100 / (rnd.NextDouble() * 100 + 1); // Exponențial
            return Math.Round((decimal)e, 2) < 1.01m ? 1.01m : Math.Round((decimal)e, 2);
        }
    }

    public class HistoryItem
    {
        public decimal Value { get; set; }
        public string ColorCode { get; set; }
    }
}