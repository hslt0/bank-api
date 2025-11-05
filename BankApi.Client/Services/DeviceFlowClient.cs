using System.Net.Http.Json;

namespace BankApi.Client.Services;

public class DeviceFlowClient(string baseUrl)
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(baseUrl) };

    public async Task<(string deviceCode, string userCode, string verificationUri)> StartDeviceFlowAsync(string clientId)
    {
        var data = new Dictionary<string, string>
        {
            ["client_id"] = clientId
        };

        var response = await _http.PostAsync("/connect/deviceauthorization", new FormUrlEncodedContent(data));
        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        
        Console.WriteLine($"User Code: {json?["user_code"]}");
        Console.WriteLine($"Verification URL: {json?["verification_uri_complete"]}");

        return (json?["device_code"]?.ToString()!, json?["user_code"]?.ToString()!, json?["verification_uri"]?.ToString()!);
    }

    public async Task VerifyAsync(string userCode, string username, string password)
    {
        var url = $"/connect/enduserverification?user_code={userCode}&username={username}&password={password}";
        var response = await _http.GetAsync(url);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}