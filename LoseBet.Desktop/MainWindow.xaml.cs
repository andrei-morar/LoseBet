using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LoseBet.Desktop
{
    public partial class MainWindow : Window
    {
        // Este un best-practice să folosim o singură instanță de HttpClient
        private static readonly HttpClient _httpClient = new HttpClient
        {
            // Setează URL-ul de bază al API-ului tău
            BaseAddress = new Uri("https://localhost:7139/")
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginData = new LoginRequest
            {
                Email = TxtEmail.Text,
                Password = TxtPassword.Password
            };

            await SendAuthRequestAsync("api/Auth/login", loginData);
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerData = new RegisterRequest
            {
                Email = TxtEmail.Text,
                Password = TxtPassword.Password,
                Cnp = TxtCnp.Text
            };

            await SendAuthRequestAsync("api/Auth/register", registerData);
        }

        // Metodă generică pentru a refolosi logica de request și tratarea erorilor
        private async Task SendAuthRequestAsync<T>(string endpoint, T payload)
        {
            try
            {
                TxtStatus.Text = "Se procesează...";
                TxtStatus.Foreground = System.Windows.Media.Brushes.Blue;

                // Trimitem request-ul asincron, serializând automat obiectul în JSON
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, payload);

                // Citim conținutul text returnat de server (opțional, pentru debugging)
                string responseMessage = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TxtStatus.Text = $"Succes! Serverul a răspuns: {response.StatusCode}\n{responseMessage}";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    TxtStatus.Text = $"Eroare! Cod HTTP: {response.StatusCode}\n{responseMessage}";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (HttpRequestException ex)
            {
                TxtStatus.Text = $"Eroare de conexiune (E pornit API-ul?):\n{ex.Message}";
                TxtStatus.Foreground = System.Windows.Media.Brushes.DarkRed;
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Eroare neașteptată:\n{ex.Message}";
                TxtStatus.Foreground = System.Windows.Media.Brushes.DarkRed;
            }
        }
    }

    // --- Modelele de date (DTOs) pentru JSON ---

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Cnp { get; set; } = string.Empty;
    }
}