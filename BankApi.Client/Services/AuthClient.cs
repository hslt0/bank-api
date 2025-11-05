using System.Net.Http.Json;
using BankApi.Client.Models;

namespace BankApi.Client.Services;

public class AuthClient
{
    private readonly HttpClient _http;

    public AuthClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<TokenResponse?> PasswordGrantAsync(string username, string password)
    {
        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username,
            ["password"] = password
        };

        var response = await _http.PostAsync("/connect/token", new FormUrlEncodedContent(data));
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Error: {await response.Content.ReadAsStringAsync()}");
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public async Task<TokenResponse?> ClientCredentialsGrantAsync(string clientId, string clientSecret)
    {
        var data = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };

        var response = await _http.PostAsync("/connect/token", new FormUrlEncodedContent(data));
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Error: {await response.Content.ReadAsStringAsync()}");
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public async Task UserInfoAsync(string accessToken)
    {
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _http.GetAsync("/connect/userinfo");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    public async Task IntrospectAsync(string token)
    {
        var data = new Dictionary<string, string>
        {
            ["token"] = token
        };

        var response = await _http.PostAsync("/connect/introspect", new FormUrlEncodedContent(data));
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    public async Task RevokeAsync(string token)
    {
        var data = new Dictionary<string, string>
        {
            ["token"] = token
        };

        var response = await _http.PostAsync("/connect/revoke", new FormUrlEncodedContent(data));
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}
