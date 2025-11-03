using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BankApi.Client.Models;

namespace BankApi.Client;

public class ApiClient
{
    private readonly HttpClient _http;
    private string? _accessToken;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task LoginAsync(string username, string password)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username,
            ["password"] = password
        };

        var response = await _http.PostAsync("/connect/token", new FormUrlEncodedContent(form));
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        _accessToken = token?.AccessToken;

        Console.WriteLine($"Logged in. Token expires in {token?.ExpiresIn}s");
    }

    public async Task GetBanksAsync()
    {
        if (_accessToken is null)
        {
            Console.WriteLine("⚠️ You must login first.");
            return;
        }

        var req = new HttpRequestMessage(HttpMethod.Get, "/banks");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _http.SendAsync(req);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var banks = JsonSerializer.Deserialize<List<Bank>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Console.WriteLine("🏦 Banks:");
        foreach (var b in banks!)
            Console.WriteLine($" - {b.Id}: {b.Name}");
    }
}