namespace BankApi.Client.Models;

public class TokenResponse
{
    public string? access_token { get; set; }
    public string? refresh_token { get; set; }
    public string? token_type { get; set; }
    public int expires_in { get; set; }

    public override string ToString()
    {
        return $"""
                Access Token: {access_token}
                Refresh Token: {refresh_token}
                Expires In: {expires_in}s
                Token Type: {token_type}
                """;
    }
}