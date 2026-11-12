using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Animal_Spa.Services
{
    public class ApiClient
    {
        public const string BaseUrl = "http://localhost:5052/";

        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };

        public async Task<(bool Success, string Message)> CheckHealthAsync()
        {
            try
            {
                using var response = await HttpClient.GetAsync("health");

                if (response.IsSuccessStatusCode)
                {
                    return (
                        true,
                        $"Connected ({(int)response.StatusCode})"
                    );
                }

                return (
                    false,
                    $"API returned {(int)response.StatusCode} " +
                    $"{response.ReasonPhrase}"
                );
            }
            catch (TaskCanceledException)
            {
                return (false, "Request timeout.");
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Cannot connect to API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }
    }
}
