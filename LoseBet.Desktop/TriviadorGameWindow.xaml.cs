using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LoseBet.Desktop
{
    public class TriviaQuestion
    {
        public string? Category { get; set; }
        public string? Text { get; set; }
        public int Type { get; set; } // 1 = Aproximare, 2 = Grilă cu 4 variante
        public string[]? Answers { get; set; }
        public string? CorrectAnswer { get; set; }
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
        private DispatcherTimer _timer = new DispatcherTimer();
        private int _timeLeft;
        private Button? _targetedTerritory;
        private TriviaQuestion? _currentQuestion;
        private Random _random = new Random();

        private Dictionary<string, List<string>> _hartăGranițe = new Dictionary<string, List<string>>();
        private List<TriviaQuestion> _questionsDB = new List<TriviaQuestion>();
        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        private class BotPlayer
        {
            public string? Name { get; set; }
            public int Score { get; set; }
            public int Territories { get; set; }
            public SolidColorBrush? Color { get; set; }
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
            this.Loaded += async (s, e) => await LoadQuestionsFromDbAsync();
            SetupTimer();
            GenerateMap();
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
                if (child is Button btn && btn.Tag?.ToString() == ownerTag)
                {
                    string ownedJudet = btn.Content.ToString()!.Split('\n')[0];
                    if (_hartăGranițe.ContainsKey(ownedJudet) && _hartăGranițe[ownedJudet].Contains(targetJudet)) return true;
                }
            }
            return false;
        }

        private async Task LoadQuestionsFromDbAsync()
        {
            try
            {
                var dbQuestions = await _httpClient.GetFromJsonAsync<List<LoseBet.Core.Models.TriviaQuestion>>("api/trivia/questions");
                if (dbQuestions != null && dbQuestions.Count > 0)
                {
                    _questionsDB.Clear();
                    foreach (var dbQ in dbQuestions)
                    {
                        var localQ = new TriviaQuestion
                        {
                            Category = dbQ.Category,
                            Text = dbQ.Text,
                            Type = dbQ.Type,
                            CorrectAnswer = dbQ.CorrectAnswer,
                            Answers = dbQ.Type == 2 ? new[] { dbQ.OptionA, dbQ.OptionB, dbQ.OptionC, dbQ.OptionD } : null
                        };
                        _questionsDB.Add(localQ);
                    }
                    _questionsDB = _questionsDB.OrderBy(q => _random.Next()).ToList();
                }
                else
                {
                    MessageBox.Show("Nu există întrebări în baza de date! Cere-i adminului să adauge.");
                }
            }
            catch (Exception)
            {
                // Silent catch
            }
        }

        private TriviaQuestion? GetNextQuestion()
        {
            if (_questionsDB.Count == 0) LoadQuestionsFromDbAsync().Wait();
            if (_questionsDB.Count == 0) return null; // Safe fallback

            TriviaQuestion q = _questionsDB[0];
            _questionsDB.RemoveAt(0);
            return q;
        }

        private void GenerateMap()
        {
            MapCanvas.Children.Clear();

            string svgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "romania.svg");

            if (!System.IO.File.Exists(svgPath))
            {
                MessageBox.Show("Fișierul romania.svg nu a fost găsit în folderul Resources!", "Eroare Harta");
                return;
            }

            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(svgPath);

                System.Xml.XmlNodeList? paths = doc.GetElementsByTagName("path");
                if (paths == null) return;

                foreach (System.Xml.XmlNode node in paths)
                {
                    if (node.Attributes?["id"] != null && node.Attributes?["d"] != null)
                    {
                        string id = node.Attributes["id"]!.Value.Replace("RO-", "");
                        string pathData = node.Attributes["d"]!.Value;
                        string fullName = node.Attributes["title"]?.Value ?? id;

                        if (_hartăGranițe.ContainsKey(id))
                        {
                            Button btnTerritory = new Button
                            {
                                Content = id,
                                Tag = "Liber",
                                // ================= ADAPTAT PENTRU DARK MODE =================
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1B22")),
                                Foreground = Brushes.White,
                                FontWeight = FontWeights.Bold,
                                FontSize = 11,
                                Cursor = System.Windows.Input.Cursors.Hand,
                                ToolTip = fullName
                            };

                            Geometry geom = Geometry.Parse(pathData);
                            Rect bounds = geom.Bounds;

                            ControlTemplate template = new ControlTemplate(typeof(Button));
                            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

                            FrameworkElementFactory pathFactory = new FrameworkElementFactory(typeof(System.Windows.Shapes.Path));
                            pathFactory.SetValue(System.Windows.Shapes.Path.DataProperty, geom);
                            pathFactory.SetValue(System.Windows.Shapes.Path.FillProperty, new TemplateBindingExtension(Button.BackgroundProperty));
                            // Marginea fiecărui județ adaptată:
                            pathFactory.SetValue(System.Windows.Shapes.Path.StrokeProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2B35")));
                            pathFactory.SetValue(System.Windows.Shapes.Path.StrokeThicknessProperty, 1.5);

                            FrameworkElementFactory presenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                            presenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                            presenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Top);

                            double textOffsetX = bounds.Left + (bounds.Width / 2) - 12;
                            double textOffsetY = bounds.Top + (bounds.Height / 2) - 10;

                            if (id == "B") { textOffsetX -= 8; textOffsetY += 5; }
                            if (id == "IF") { textOffsetX += 8; textOffsetY -= 8; }

                            presenterFactory.SetValue(ContentPresenter.MarginProperty, new Thickness(textOffsetX, textOffsetY, 0, 0));

                            gridFactory.AppendChild(pathFactory);
                            gridFactory.AppendChild(presenterFactory);
                            template.VisualTree = gridFactory;

                            btnTerritory.Template = template;
                            btnTerritory.Click += BtnTerritory_Click;

                            MapCanvas.Children.Add(btnTerritory);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silent catch
            }
        }

        private void BtnTerritory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickedBtn) return;
            string judetNume = clickedBtn.Content.ToString()!.Split('\n')[0];

            if (_gamePhase == 0)
            {
                // Culoarea jucătorului (Blue Neon)
                clickedBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2979FF"));
                clickedBtn.Foreground = Brushes.White;
                clickedBtn.Tag = "Tu";
                clickedBtn.Content = $"{judetNume}\n(👑)";

                _playerTerritories++; _playerScore += 300;
                PlaceBotCapitals();
                _gamePhase = 1; TxtMapStatus.Text = "FAZA 2: Atacă județele vecine libere!";
                UpdateScoreboard();
            }
            else if (_gamePhase == 1)
            {
                if (clickedBtn.Tag?.ToString() == "Liber")
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
            if (_currentQuestion == null) return;

            TxtQuestion.Text = _currentQuestion.Text;
            MapCanvas.IsEnabled = false;

            if (_currentQuestion.Type == 2 && _currentQuestion.Answers != null)
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
            if (sender is not Button btn) return;
            string selectedAnswer = btn.Content.ToString()!;

            if (selectedAnswer == _currentQuestion?.CorrectAnswer) EndBattle(true, "Răspuns Corect! Ai cucerit județul.");
            else EndBattle(false, $"Răspuns Greșit! Cel corect era: {_currentQuestion?.CorrectAnswer}");
        }

        private void BtnSubmitApproximation_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            if (int.TryParse(TxtAnswerInput.Text.Trim(), out int playerAns)) ProcessApproximationResult(playerAns);
            else { MessageBox.Show("Te rog introdu un număr valid!", "Eroare"); _timer.Start(); }
        }

        private void ProcessApproximationResult(int playerAnswer)
        {
            if (_currentQuestion?.CorrectAnswer == null || _targetedTerritory == null) return;

            int correctAnswer = int.Parse(_currentQuestion.CorrectAnswer);
            bool isNeutral = _targetedTerritory.Tag?.ToString() == "Liber";

            if (isNeutral)
            {
                int allowedMargin = (int)(correctAnswer * 0.15);
                if (allowedMargin < 5) allowedMargin = 5;

                int diff = Math.Abs(correctAnswer - playerAnswer);
                if (diff <= allowedMargin) EndBattle(true, $"Cucerit! Răspunsul era {correctAnswer} (Aproximare acceptată).");
                else EndBattle(false, $"Ai greșit! Răspunsul exact era {correctAnswer}.");
            }
        }

        private async void EndBattle(bool won, string message)
        {
            PanelMultipleChoice.Visibility = Visibility.Collapsed;
            PanelApproximation.Visibility = Visibility.Collapsed;
            MapCanvas.IsEnabled = true;

            if (won && _targetedTerritory != null)
            {
                MessageBox.Show(message, "Victorie!");
                _targetedTerritory.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2979FF"));
                _targetedTerritory.Foreground = Brushes.White;
                _targetedTerritory.Tag = "Tu";
                _playerScore += 100; _playerTerritories++;
            }
            else MessageBox.Show(message, "Luptă pierdută! Teritoriul rămâne liber.");

            UpdateScoreboard();

            if (HasFreeTerritories()) { _gamePhase = 2; await RunBotsTurn(); }
            else FinishMatch();
        }

        private void InitializeBots()
        {
            // Culori neon pentru boți ca să se potrivească pe mapa dark
            SolidColorBrush[] botColors = {
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935")), // Roșu
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676")), // Verde
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")), // Portocaliu
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D500F9")), // Mov
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F50057"))  // Roz
            };

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
                    if (MapCanvas.Children[index] is Button btn && btn.Tag?.ToString() == "Liber")
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
                    if (child is Button b && b.Tag?.ToString() == "Liber" && IsAdjacent(b.Content.ToString()!.Split('\n')[0], bot.Name!))
                        validTargets.Add(b);
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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timeLeft--; TxtTimer.Text = $"⏳ {_timeLeft}";
            if (_timeLeft <= 0)
            {
                _timer.Stop();
                if (_currentQuestion?.Type == 2) EndBattle(false, "Timpul a expirat! Ai pierdut.");
                else ProcessApproximationResult(0);
            }
        }

        private bool HasFreeTerritories() { foreach (var child in MapCanvas.Children) { if (child is Button b && b.Tag?.ToString() == "Liber") return true; } return false; }

        private void UpdateScoreboard()
        {
            TxtPlayerScore.Text = $"TU: {_playerScore} pct | Teritorii: {_playerTerritories}";
            string botsInfo = "BOȚI: ";
            foreach (var bot in _botsList) botsInfo += $"{bot.Name} ({bot.Score}p / {bot.Territories}t) | ";
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