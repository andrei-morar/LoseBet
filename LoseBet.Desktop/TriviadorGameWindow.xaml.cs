using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LoseBet.Desktop
{
    public class TriviaQuestion
    {
        public string Category { get; set; }
        public string Text { get; set; }
        public int Type { get; set; } // 1 = Aproximare, 2 = Grilă cu 4 variante
        public string[] Answers { get; set; }
        public string CorrectAnswer { get; set; }
    }

    public partial class TriviadorGameWindow : Window
    {
        private string _username;
        private decimal _balance;
        private decimal _betAmount;
        private int _botCount;

        private int _gamePhase = 0;
        private int _playerScore = 0;
        private int _playerTerritories = 0;

        private List<BotPlayer> _botsList = new List<BotPlayer>();
        private DispatcherTimer _timer;
        private int _timeLeft;
        private Button _targetedTerritory;
        private TriviaQuestion _currentQuestion;
        private Random _random = new Random();

        private Dictionary<string, List<string>> _hartăGranițe = new Dictionary<string, List<string>>();
        private string[] _judete = { "AB", "AR", "AG", "BC", "BH", "BN", "BT", "BV", "BR", "BZ", "CS", "CL", "CJ", "CT", "CV", "DB", "DJ", "GL", "GR", "GJ", "HR", "HD", "IL", "IS", "IF", "MM", "MH", "MS", "NT", "OT", "PH", "SM", "SJ", "SB", "SV", "TR", "TM", "TL", "VS", "VL", "VN", "B" };
        private List<TriviaQuestion> _questionsDB = new List<TriviaQuestion>();

        private class BotPlayer
        {
            public string Name { get; set; }
            public int Score { get; set; }
            public int Territories { get; set; }
            public SolidColorBrush Color { get; set; }
        }

        public TriviadorGameWindow(string username, decimal balance, decimal betAmount, int botCount)
        {
            InitializeComponent();
            _username = username;
            _balance = balance;
            _betAmount = betAmount;
            _botCount = botCount;

            TxtMiza.Text = $"Miză: {_betAmount} RON | Boți: {_botCount}";

            BuildBorders();
            InitializeBots();
            LoadQuestions();
            SetupTimer();
            GenerateMap(); // AICI AM SCHIMBAT! Generăm harta vectorială
        }

        private void BuildBorders()
        {
            _hartăGranițe["AB"] = new List<string> { "AR", "BH", "CJ", "MS", "SB", "HD", "VL" };
            _hartăGranițe["AR"] = new List<string> { "BH", "AB", "HD", "TM" };
            _hartăGranițe["AG"] = new List<string> { "SB", "BV", "DB", "TR", "OT", "VL" };
            _hartăGranițe["BC"] = new List<string> { "NT", "IS", "VS", "VN", "CV", "HR" };
            _hartăGranițe["BH"] = new List<string> { "SM", "SJ", "CJ", "AB", "AR" };
            _hartăGranițe["BN"] = new List<string> { "MM", "SV", "MS", "CJ" };
            _hartăGranițe["BT"] = new List<string> { "SV", "IS" };
            _hartăGranițe["BV"] = new List<string> { "MS", "HR", "CV", "BZ", "PH", "DB", "AG", "SB" };
            _hartăGranițe["BR"] = new List<string> { "GL", "TL", "CT", "IL", "BZ", "VN" };
            _hartăGranițe["BZ"] = new List<string> { "VN", "BR", "IL", "PH", "BV", "CV" };
            _hartăGranițe["CS"] = new List<string> { "TM", "HD", "GJ", "MH" };
            _hartăGranițe["CL"] = new List<string> { "IL", "CT", "GR", "IF" };
            _hartăGranițe["CJ"] = new List<string> { "SJ", "MM", "BN", "MS", "AB", "BH" };
            _hartăGranițe["CT"] = new List<string> { "TL", "BR", "IL", "CL" };
            _hartăGranițe["CV"] = new List<string> { "HR", "BC", "VN", "BZ", "BV" };
            _hartăGranițe["DB"] = new List<string> { "BV", "PH", "IF", "GR", "TR", "AG" };
            _hartăGranițe["DJ"] = new List<string> { "MH", "GJ", "VL", "OT" };
            _hartăGranițe["GL"] = new List<string> { "VS", "VN", "BR", "TL" };
            _hartăGranițe["GR"] = new List<string> { "TR", "DB", "IF", "CL", "B" };
            _hartăGranițe["GJ"] = new List<string> { "MH", "CS", "HD", "VL", "DJ" };
            _hartăGranițe["HR"] = new List<string> { "SV", "NT", "BC", "CV", "BV", "MS" };
            _hartăGranițe["HD"] = new List<string> { "AR", "AB", "SB", "VL", "GJ", "CS", "TM" };
            _hartăGranițe["IL"] = new List<string> { "BR", "CT", "CL", "IF", "PH", "BZ" };
            _hartăGranițe["IS"] = new List<string> { "BT", "SV", "NT", "VS", "BC" };
            _hartăGranițe["IF"] = new List<string> { "PH", "IL", "CL", "GR", "DB", "B" };
            _hartăGranițe["MM"] = new List<string> { "SM", "SJ", "CJ", "BN", "SV" };
            _hartăGranițe["MH"] = new List<string> { "CS", "GJ", "DJ" };
            _hartăGranițe["MS"] = new List<string> { "BN", "HR", "BV", "SB", "AB", "CJ" };
            _hartăGranițe["NT"] = new List<string> { "SV", "IS", "BC", "HR" };
            _hartăGranițe["OT"] = new List<string> { "DJ", "VL", "AG", "TR" };
            _hartăGranițe["PH"] = new List<string> { "BV", "BZ", "IL", "IF", "DB" };
            _hartăGranițe["SM"] = new List<string> { "BH", "SJ", "MM" };
            _hartăGranițe["SJ"] = new List<string> { "SM", "MM", "CJ", "BH" };
            _hartăGranițe["SB"] = new List<string> { "MS", "BV", "AG", "VL", "AB" };
            _hartăGranițe["SV"] = new List<string> { "BT", "IS", "NT", "HR", "BN", "MM" };
            _hartăGranițe["TR"] = new List<string> { "OT", "AG", "DB", "GR" };
            _hartăGranițe["TM"] = new List<string> { "AR", "HD", "CS" };
            _hartăGranițe["TL"] = new List<string> { "GL", "BR", "CT" };
            _hartăGranițe["VS"] = new List<string> { "IS", "BC", "VN", "GL" };
            _hartăGranițe["VL"] = new List<string> { "HD", "SB", "AG", "OT", "DJ", "GJ", "AB" };
            _hartăGranițe["VN"] = new List<string> { "BC", "VS", "GL", "BR", "BZ", "CV" };
            _hartăGranițe["B"] = new List<string> { "IF", "GR" };
        }

        private bool IsAdjacent(string targetJudet, string ownerTag)
        {
            foreach (var child in MapCanvas.Children)
            {
                Button btn = child as Button;
                if (btn != null && btn.Tag.ToString() == ownerTag)
                {
                    string ownedJudet = btn.Content.ToString().Split('\n')[0];
                    if (_hartăGranițe.ContainsKey(ownedJudet) && _hartăGranițe[ownedJudet].Contains(targetJudet)) return true;
                }
            }
            return false;
        }

        private void LoadQuestions()
        {
            _questionsDB.Clear();
            _questionsDB.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Cine a fost primul domnitor al Țării Românești?", Answers = new[] { "Mircea cel Bătrân", "Basarab I", "Vlad Țepeș", "Mihai Viteazul" }, CorrectAnswer = "Basarab I" });
            _questionsDB.Add(new TriviaQuestion { Category = "Istorie", Type = 2, Text = "Ce domnitor a realizat prima unire a Țărilor Române?", Answers = new[] { "Mihai Viteazul", "Alexandru Ioan Cuza", "Ștefan cel Mare", "Carol I" }, CorrectAnswer = "Mihai Viteazul" });
            _questionsDB.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a avut loc Marea Unire de la Alba Iulia?", CorrectAnswer = "1918" });
            _questionsDB.Add(new TriviaQuestion { Category = "Istorie", Type = 1, Text = "În ce an a început Primul Război Mondial?", CorrectAnswer = "1914" });

            _questionsDB.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "Care este cel mai înalt vârf muntos din România?", Answers = new[] { "Omu", "Negoiu", "Moldoveanu", "Peleaga" }, CorrectAnswer = "Moldoveanu" });
            _questionsDB.Add(new TriviaQuestion { Category = "Geografie", Type = 2, Text = "În ce mare se varsă fluviul Dunărea?", Answers = new[] { "Marea Neagră", "Marea Mediterană", "Marea Roșie", "Marea Caspică" }, CorrectAnswer = "Marea Neagră" });
            _questionsDB.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Care este altitudinea vârfului Moldoveanu (în metri)?", CorrectAnswer = "2544" });
            _questionsDB.Add(new TriviaQuestion { Category = "Geografie", Type = 1, Text = "Câte județe are România (fără București)?", CorrectAnswer = "41" });

            _questionsDB = _questionsDB.OrderBy(q => _random.Next()).ToList();
        }

        private TriviaQuestion GetNextQuestion()
        {
            if (_questionsDB.Count == 0) LoadQuestions();
            TriviaQuestion q = _questionsDB[0];
            _questionsDB.RemoveAt(0);
            return q;
        }

        // ================= GENERAREA HĂRȚII VECTORIALE (SVG) =================
        private void GenerateMap()
        {
            MapCanvas.Children.Clear();

            // Aici introduci coordonatele SVG reale pentru fiecare județ!
            // Formatele sunt de tip: "M 10,20 L 30,40 Z" etc.
            var svgHartaRomania = new Dictionary<string, string>
            {
                // Am pus un pătrat provizoriu pentru CJ ca să vezi funcționalitatea.
                // Tu va trebui să le înlocuiești pe toate 42 cu path-urile din SVG-ul tău.
                { "CJ", "M 220,130 L 260,120 L 280,160 L 250,190 L 210,170 Z" },
                { "AB", "M 210,170 L 250,190 L 240,240 L 190,220 L 180,190 Z" },
                { "BH", "M 160,110 L 220,130 L 210,170 L 180,190 L 150,160 Z" }
            };

            foreach (var judet in _judete)
            {
                // Daca nu ai gasit inca SVG-ul pt județ, ii facem un cerc generic să nu crape
                string pathData = svgHartaRomania.ContainsKey(judet) ? svgHartaRomania[judet] : "M 0,0 a 15,15 0 1,0 30,0 a 15,15 0 1,0 -30,0";

                Button btnTerritory = new Button
                {
                    Content = judet,
                    Tag = "Liber",
                    Background = Brushes.LightGray,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    FontSize = 10,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    ToolTip = $"Județul {judet}"
                };

                // CREĂM ȘABLONUL VECTORIAL PENTRU BUTON
                ControlTemplate template = new ControlTemplate(typeof(Button));
                FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

                // Desenăm granițele
                FrameworkElementFactory pathFactory = new FrameworkElementFactory(typeof(System.Windows.Shapes.Path));
                pathFactory.SetValue(System.Windows.Shapes.Path.DataProperty, Geometry.Parse(pathData));

                // Aici conectăm Background-ul butonului cu culoarea din interiorul județului (Când e albastru se face teritoriul albastru)
                pathFactory.SetValue(System.Windows.Shapes.Path.FillProperty, new TemplateBindingExtension(Button.BackgroundProperty));
                pathFactory.SetValue(System.Windows.Shapes.Path.StrokeProperty, Brushes.White); // Conturul alb dintre județe
                pathFactory.SetValue(System.Windows.Shapes.Path.StrokeThicknessProperty, 1.5);

                // Afișăm textul (numele județului) fix peste desen
                FrameworkElementFactory presenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                presenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                presenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

                gridFactory.AppendChild(pathFactory);
                gridFactory.AppendChild(presenterFactory);
                template.VisualTree = gridFactory;

                btnTerritory.Template = template;
                btnTerritory.Click += BtnTerritory_Click;

                // Nu mai setăm X și Y manual! Formele SVG știu deja unde trebuie să stea.
                // Excepție fac cele la care nu le-ai pus path-ul încă (acele cercuri). Pentru ele facem o aranjare basic temporară.
                if (!svgHartaRomania.ContainsKey(judet))
                {
                    Canvas.SetLeft(btnTerritory, _random.Next(50, 500));
                    Canvas.SetTop(btnTerritory, _random.Next(50, 350));
                }

                MapCanvas.Children.Add(btnTerritory);
            }
        }

        private void BtnTerritory_Click(object sender, RoutedEventArgs e)
        {
            Button clickedBtn = sender as Button;
            string judetNume = clickedBtn.Content.ToString().Split('\n')[0];

            if (_gamePhase == 0)
            {
                clickedBtn.Background = Brushes.DodgerBlue; clickedBtn.Foreground = Brushes.White; clickedBtn.Tag = "Tu"; clickedBtn.Content = $"{judetNume}\n(👑)";
                _playerTerritories++; _playerScore += 300;
                PlaceBotCapitals();
                _gamePhase = 1; TxtMapStatus.Text = "FAZA 2: Atacă județele vecine libere!";
                UpdateScoreboard();
            }
            else if (_gamePhase == 1)
            {
                if (clickedBtn.Tag.ToString() == "Liber")
                {
                    if (IsAdjacent(judetNume, "Tu"))
                    {
                        _targetedTerritory = clickedBtn;
                        StartBattle();
                    }
                    else MessageBox.Show("Strategie greșită! Poți ataca doar teritorii care au graniță comună cu cele pe care le deții deja!", "Eroare Tactică");
                }
                else MessageBox.Show("Poți ataca doar teritoriile libere (gri)!", "Atenție");
            }
        }

        private void StartBattle()
        {
            _currentQuestion = GetNextQuestion();
            TxtQuestion.Text = _currentQuestion.Text;
            MapCanvas.IsEnabled = false;

            if (_currentQuestion.Type == 2)
            {
                _timeLeft = 30;
                PanelMultipleChoice.Visibility = Visibility.Visible;
                PanelApproximation.Visibility = Visibility.Collapsed;

                BtnA.Content = _currentQuestion.Answers[0]; BtnB.Content = _currentQuestion.Answers[1];
                BtnC.Content = _currentQuestion.Answers[2]; BtnD.Content = _currentQuestion.Answers[3];
            }
            else if (_currentQuestion.Type == 1)
            {
                _timeLeft = 40;
                PanelMultipleChoice.Visibility = Visibility.Collapsed;
                PanelApproximation.Visibility = Visibility.Visible;
                TxtAnswerInput.Text = "";
            }

            TxtTimer.Text = $"⏳ {_timeLeft}";
            _timer.Start();
        }

        private void BtnAnswer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Button btn = sender as Button;
            string selectedAnswer = btn.Content.ToString();

            if (selectedAnswer == _currentQuestion.CorrectAnswer) EndBattle(true, "Răspuns Corect! Ai cucerit județul.");
            else EndBattle(false, $"Răspuns Greșit! Cel corect era: {_currentQuestion.CorrectAnswer}");
        }

        private void BtnSubmitApproximation_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            if (int.TryParse(TxtAnswerInput.Text.Trim(), out int playerAns)) ProcessApproximationResult(playerAns);
            else { MessageBox.Show("Te rog introdu un număr valid!", "Eroare"); _timer.Start(); }
        }

        private void ProcessApproximationResult(int playerAnswer)
        {
            int correctAnswer = int.Parse(_currentQuestion.CorrectAnswer);
            bool isNeutral = _targetedTerritory.Tag.ToString() == "Liber";

            if (isNeutral)
            {
                int allowedMargin = (int)(correctAnswer * 0.15);
                if (allowedMargin < 5) allowedMargin = 5;

                int diff = Math.Abs(correctAnswer - playerAnswer);
                if (diff <= allowedMargin) EndBattle(true, $"Cucerit! Răspunsul era {correctAnswer} (Aproximare perfectă!).");
                else EndBattle(false, $"Ai greșit! Răspunsul exact era {correctAnswer}.");
            }
        }

        private async void EndBattle(bool won, string message)
        {
            PanelMultipleChoice.Visibility = Visibility.Collapsed;
            PanelApproximation.Visibility = Visibility.Collapsed;
            MapCanvas.IsEnabled = true;

            if (won)
            {
                MessageBox.Show(message, "Victorie!");
                _targetedTerritory.Background = Brushes.DodgerBlue; _targetedTerritory.Foreground = Brushes.White; _targetedTerritory.Tag = "Tu";
                _playerScore += 100; _playerTerritories++;
            }
            else MessageBox.Show(message, "Luptă pierdută! Teritoriul rămâne liber.");

            UpdateScoreboard();

            if (HasFreeTerritories()) { _gamePhase = 2; await RunBotsTurn(); }
            else FinishMatch();
        }

        private void InitializeBots()
        {
            SolidColorBrush[] botColors = { Brushes.Crimson, Brushes.ForestGreen, Brushes.Orange, Brushes.Purple, Brushes.Magenta };
            for (int i = 0; i < _botCount; i++) _botsList.Add(new BotPlayer { Name = $"Bot {i + 1}", Score = 0, Territories = 0, Color = botColors[i % botColors.Length] });
        }

        private void PlaceBotCapitals()
        {
            foreach (var bot in _botsList)
            {
                bool placed = false;
                while (!placed)
                {
                    int index = _random.Next(0, MapCanvas.Children.Count);
                    Button btn = MapCanvas.Children[index] as Button;
                    if (btn.Tag.ToString() == "Liber")
                    {
                        btn.Background = bot.Color; btn.Foreground = Brushes.White; btn.Tag = bot.Name; btn.Content = $"{btn.Content}\n(🤖)";
                        bot.Territories++; bot.Score += 300; placed = true;
                    }
                }
            }
        }

        private async Task RunBotsTurn()
        {
            TxtMapStatus.Text = "🤖 Boții își extind imperiul...";
            MapCanvas.IsEnabled = false;

            foreach (var bot in _botsList)
            {
                if (!HasFreeTerritories()) break;
                await Task.Delay(1500);

                List<Button> validTargets = new List<Button>();
                foreach (var child in MapCanvas.Children)
                {
                    Button b = child as Button;
                    if (b.Tag.ToString() == "Liber" && IsAdjacent(b.Content.ToString().Split('\n')[0], bot.Name)) validTargets.Add(b);
                }

                if (validTargets.Count > 0)
                {
                    Button target = validTargets[_random.Next(validTargets.Count)];
                    if (_random.Next(0, 100) < 65)
                    {
                        target.Background = bot.Color; target.Foreground = Brushes.White; target.Tag = bot.Name; bot.Score += 100; bot.Territories++;
                    }
                }
                UpdateScoreboard();
            }

            if (HasFreeTerritories()) { _gamePhase = 1; TxtMapStatus.Text = "Este rândul tău! Atacă un județ VECIN."; MapCanvas.IsEnabled = true; }
            else FinishMatch();
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer(); _timer.Interval = TimeSpan.FromSeconds(1); _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft--; TxtTimer.Text = $"⏳ {_timeLeft}";
            if (_timeLeft <= 0)
            {
                _timer.Stop();
                if (_currentQuestion.Type == 2) EndBattle(false, "Timpul a expirat! Ai pierdut.");
                else ProcessApproximationResult(0);
            }
        }

        private bool HasFreeTerritories() { foreach (var child in MapCanvas.Children) { Button b = child as Button; if (b.Tag.ToString() == "Liber") return true; } return false; }

        private void UpdateScoreboard()
        {
            TxtPlayerScore.Text = $"TU: {_playerScore} pct | Județe: {_playerTerritories}";
            string botsInfo = "BOȚI: ";
            foreach (var bot in _botsList) botsInfo += $"{bot.Name} ({bot.Score}p / {bot.Territories}J) | ";
            if (botsInfo.EndsWith(" | ")) botsInfo = botsInfo.Substring(0, botsInfo.Length - 3);
            TxtBotsScore.Text = botsInfo;
        }

        private void FinishMatch()
        {
            _gamePhase = 3; _timer.Stop(); MapCanvas.IsEnabled = false;
            var playerNode = new { Name = "Tu", Score = _playerScore };
            var leaderboard = _botsList.Select(b => new { Name = b.Name, Score = b.Score }).ToList();
            leaderboard.Add(playerNode);
            leaderboard = leaderboard.OrderByDescending(x => x.Score).ToList();

            int playerRank = leaderboard.FindIndex(x => x.Name == "Tu") + 1;
            decimal payout = 0;
            string rankMessage = $"Harta a fost cucerită complet!\nAi obținut locul {playerRank} din {leaderboard.Count}.\n\n";

            if (_botCount == 1) { if (playerRank == 1) { payout = _betAmount * 4; rankMessage += $"🥇 Câștigi x4: {payout} RON!"; } else rankMessage += "❌ Pierzi miza."; }
            else if (_botCount == 2) { if (playerRank == 1) { payout = _betAmount * 3; rankMessage += $"🥇 Câștigi x3: {payout} RON!"; } else if (playerRank == 2) { payout = _betAmount; rankMessage += $"🥈 Recuperezi miza: {payout} RON."; } else rankMessage += "❌ Pierzi miza."; }
            else if (_botCount == 5) { if (playerRank == 1) { payout = _betAmount * 5; rankMessage += $"🏆 CAMPION! Câștigi x5: {payout} RON!"; } else if (playerRank == 2) { payout = _betAmount * 1.5m; rankMessage += $"🥈 Locul 2: {payout} RON."; } else if (playerRank == 3) { payout = _betAmount; rankMessage += $"🥉 Locul 3: Banii înapoi ({payout} RON)."; } else if (playerRank == 4) { payout = _betAmount / 2; rankMessage += $"📉 Locul 4: Salvezi {payout} RON."; } else rankMessage += "❌ Pierzi tot."; }

            _balance += payout;
            MessageBox.Show(rankMessage, "Rezultat Final Turneu");
            new DashboardWindow(_username, _balance.ToString()).Show(); this.Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Abandonezi? Vei pierde miza!", "Atenție", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                new DashboardWindow(_username, _balance.ToString()).Show(); this.Close();
            }
        }
    }
}