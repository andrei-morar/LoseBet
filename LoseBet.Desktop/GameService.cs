using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoseBet.Desktop
{
    public static class GameService
    {
        private static readonly HttpClient _client = new HttpClient { BaseAddress = new Uri("https://localhost:7000/") };

        // Megafonul care anunță schimbarea balanței
        public static event Action BalanceUpdated;

        public static void NotifyBalanceChanged()
        {
            BalanceUpdated?.Invoke();
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // 1. Ia balanța din WalletController
        public static async Task<decimal> GetBalanceAsync()
        {
            try
            {
                var response = await _client.GetAsync($"api/Wallet/balance/{UserSession.UserId}");

                string content = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<BalanceResponse>(content, _jsonOptions);
                    return result?.Balance ?? 0;
                }
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        // ================= MINES =================
        public static async Task<MinesStartResult> StartMinesAsync(decimal betAmount, int bombCount)
        {
            var body = new { UserId = UserSession.UserId, BetAmount = betAmount, BombCount = bombCount };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/FastGames/mines/start", content);

            if (!response.IsSuccessStatusCode) return null;

            var resultStr = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MinesStartResult>(resultStr, _jsonOptions);
        }

        public static async Task<MinesPickResult> PickMineAsync(int sessionId, int index)
        {
            var body = new { SessionId = sessionId, Index = index };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/FastGames/mines/pick", content);

            var resultStr = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MinesPickResult>(resultStr, _jsonOptions);
        }

        public static async Task<decimal> CashoutMinesAsync(int sessionId)
        {
            var body = new { SessionId = sessionId };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/FastGames/mines/cashout", content);

            if (response.IsSuccessStatusCode)
            {
                var resultStr = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(resultStr);
                return data.GetProperty("winAmount").GetDecimal();
            }
            return 0;
        }

        // ================= AVIATOR =================
        public static async Task<AviatorStartResult> StartAviatorAsync(decimal betAmount)
        {
            var body = new { UserId = UserSession.UserId, BetAmount = betAmount };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/FastGames/aviator/start", content);

            if (!response.IsSuccessStatusCode) return null;

            var resultStr = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AviatorStartResult>(resultStr, _jsonOptions);
        }

        public static async Task<AviatorCashoutResult> CashoutAviatorAsync(int sessionId, decimal multiplier)
        {
            var body = new { SessionId = sessionId, ClaimedMultiplier = multiplier };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/FastGames/aviator/cashout", content);

            var resultStr = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AviatorCashoutResult>(resultStr, _jsonOptions);
        }
    }

    // Clase pentru a citi răspunsurile API-ului tău
    public class MinesStartResult { public int SessionId { get; set; } }
    public class MinesPickResult { public bool HitBomb { get; set; } public decimal CurrentMultiplier { get; set; } public string Message { get; set; } }
    public class AviatorStartResult { public int SessionId { get; set; } public decimal CrashPoint { get; set; } }
    public class AviatorCashoutResult { public bool Success { get; set; } public decimal WinAmount { get; set; } public string Message { get; set; } }
    public class BalanceResponse { public decimal Balance { get; set; } }
}