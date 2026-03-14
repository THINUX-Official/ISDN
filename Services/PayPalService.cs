using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ISDN.Services
{
    public class PayPalService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public PayPalService(IConfiguration config, HttpClient http)
        {
            _config = config;
            _http = http;
            var env = (_config["PayPal:Environment"] ?? "sandbox").ToLower();
            _baseUrl = env == "live" ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            var clientId = _config["PayPal:ClientId"];
            var secret = _config["PayPal:Secret"];
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(secret)) return null;

            var req = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/v1/oauth2/token");
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + secret));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            req.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("access_token", out var t)) return t.GetString();
            return null;
        }

        public async Task<(bool Success, JsonDocument? OrderJson)> VerifyOrderAsync(string orderId)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token)) return (false, null);

                var req = new HttpRequestMessage(HttpMethod.Get, _baseUrl + $"/v2/checkout/orders/{orderId}");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = await _http.SendAsync(req);
                if (!res.IsSuccessStatusCode) return (false, null);
                var json = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                return (true, doc);
            }
            catch
            {
                return (false, null);
            }
        }

        public async Task<(bool Success, JsonDocument? RefundJson)> RefundCaptureAsync(string captureId, string amount, string currency = "USD")
        {
            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token)) return (false, null);

                var url = _baseUrl + $"/v2/payments/captures/{captureId}/refund";
                var body = JsonSerializer.Serialize(new { amount = new { value = amount, currency_code = currency } });
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent(body, Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req);
                var json = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode) return (false, JsonDocument.Parse(json));
                return (true, JsonDocument.Parse(json));
            }
            catch
            {
                return (false, null);
            }
        }
    }
}
