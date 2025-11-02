using OpenIddict.EntityFrameworkCore.Models;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    
    public string Role { get; set; } = "user";

    public ICollection<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; } = new List<OpenIddictEntityFrameworkCoreToken>();
}