using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SDAHymns.Core.Services
{
    public class BroadcastSyncService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string SyncUrl = "http://127.0.0.1:4545/sync";

        public async Task SyncHymnAsync(int number, string title)
        {
            try
            {
                var payload = new
                {
                    hymnNumber = number.ToString(),
                    hymnTitle = title,
                    hymnVisible = true
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(SyncUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log error but don't crash the main app
                Console.WriteLine($"Broadcast Sync Error: {ex.Message}");
            }
        }

        public async Task HideHymnAsync()
        {
            try
            {
                var payload = new { hymnVisible = false };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync(SyncUrl, content);
            }
            catch { /* Ignore */ }
        }
    }
}
