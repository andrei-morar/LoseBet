using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LoseBet.Desktop
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7000/")
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- BUTOANE PENTRU SCHIMBAREA ECRANULUI ---
        private void BtnShowRegister_Click(object sender, RoutedEventArgs e)
        {
            PnlLogin.Visibility = Visibility.Collapsed;
            PnlRegister.Visibility = Visibility.Visible;
            TxtStatus.Text = "";
        }

        private void BtnShowLogin_Click(object sender, RoutedEventArgs e)
        {
            PnlRegister.Visibility = Visibility.Collapsed;
            PnlLogin.Visibility = Visibility.Visible;
            TxtStatus.Text = "";
        }

        // --- LOGICA DE TRIMITERE DATE ---
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginData = new LoginRequest
            {
                Username = TxtLoginUsername.Text,
                Password = TxtLoginPassword.Password
            };

            await SendAuthRequestAsync("api/Auth/login", loginData);
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerData = new RegisterRequest
            {
                Nume = TxtRegNume.Text,
                Prenume = TxtRegPrenume.Text,
                Cnp = TxtRegCnp.Text,
                DataNasterii = DpDataNasterii.SelectedDate ?? DateTime.Now,
                Email = TxtRegEmail.Text,
                Username = TxtRegUsername.Text,
                Password = TxtRegPassword.Password
            };

            await SendAuthRequestAsync("api/Auth/register", registerData);
        }

        private async Task SendAuthRequestAsync<T>(string endpoint, T payload)
        {
            try
            {
                TxtStatus.Text = "Se procesează...";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Blue;

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, payload);
                string responseMessage = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // VERIFICĂM CE FEL DE REQUEST A FOST (Login sau Register?)
                    if (endpoint.Contains("login"))
                    {
                        // Citim răspunsul complet de la API sub formă de obiect
                        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();

                        if (loginResult != null)
                        {
                            // Trimitem datele REALE către Dashboard
                            DashboardWindow dashboard = new DashboardWindow(loginResult.Username, loginResult.Balance.ToString("0.00"));
                            dashboard.Show();
                            this.Close();
                        }
                    }
                    else if (endpoint.Contains("register"))
                    {
                        // 1. E REGISTER: Afișăm un mesaj frumos
                        TxtStatus.Text = "Cont creat cu succes! Acum te poți autentifica.";
                        TxtStatus.Foreground = System.Windows.Media.Brushes.Green;

                        // 2. Mutăm utilizatorul înapoi pe formularul de Login
                        PnlRegister.Visibility = Visibility.Collapsed;
                        PnlLogin.Visibility = Visibility.Visible;

                        // Curățăm parolele pentru siguranță
                        TxtLoginPassword.Password = "";
                        TxtRegPassword.Password = "";
                    }
                }
                else
                {
                    // Dacă API-ul ne dă eroare (ex: parola greșită, sub 18 ani)
                    TxtStatus.Text = $"Eroare! Cod HTTP: {response.StatusCode}\n{responseMessage}";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Eroare:\n{ex.Message}";
                TxtStatus.Foreground = System.Windows.Media.Brushes.DarkRed;
            }
        }
    }

    // --- Modelele de date (DTOs) ---

    public class LoginResponse
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public decimal Balance { get; set; }
        public string Role { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string Cnp { get; set; } = string.Empty;
        public DateTime DataNasterii { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}