using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server.AspNetCore;

public static class OauthOperation
{
    public static async Task<IResult> GetToken(HttpContext context, OAuthDb db)
    {
        var request = context.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("Cannot get OpenIddict request.");

        if (request.IsPasswordGrantType())
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid username or password."
                    }));
            }

            var principal = CreatePrincipal(user.Username, user.Role);
            principal.SetScopes(request.GetScopes());
            return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            var principal = CreatePrincipal(request.ClientId!, "Service Account");
            principal.SetScopes(request.GetScopes());
            return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = result.Principal ?? throw new InvalidOperationException("Invalid refresh token.");

            // Optionally revalidate user existence
            return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Results.BadRequest(new { error = "Unsupported grant type." });
    }

    public static IResult Authorize(HttpContext context)
    {
        var request = context.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("Invalid OpenID request.");

        var principal = CreatePrincipal("admin", "Administrator");
        principal.SetScopes(request.GetScopes());
        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    public static IResult Logout(HttpContext context)
    {
        return Results.SignOut(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    public static async Task<IResult> Introspect(HttpContext context, OAuthDb db)
    {
        var request = context.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("Invalid introspection request.");

        var tokenValue = request.Token;
        if (string.IsNullOrWhiteSpace(tokenValue))
            return Results.BadRequest(new { error = "Missing token" });

        var tokenEntity = await db.Set<OpenIddictEntityFrameworkCoreToken>()
            .Include(t => t.Application)
            .FirstOrDefaultAsync(t => t.ReferenceId == tokenValue || t.Payload == tokenValue);

        if (tokenEntity == null)
            return Results.Ok(new { active = false });

        if (tokenEntity.Status == OpenIddictConstants.Statuses.Revoked ||
            (tokenEntity.ExpirationDate != null && tokenEntity.ExpirationDate < DateTime.UtcNow))
        {
            return Results.Ok(new { active = false });
        }

        string? scopeString = null;
        if (!string.IsNullOrEmpty(tokenEntity.Payload))
        {
            try
            {
                using var json = JsonDocument.Parse(tokenEntity.Payload);
                if (json.RootElement.TryGetProperty("scope", out var scopeElement))
                {
                    scopeString = scopeElement.GetString();
                }
            }
            catch
            {
                // ignore malformed payloads
            }
        }

        string? username = null;
        if (tokenEntity.Subject != null)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == tokenEntity.Subject);
            username = user?.Username ?? tokenEntity.Subject;
        }

        return Results.Ok(new
        {
            active = true,
            username,
            client_id = tokenEntity.Application?.ClientId,
            scope = scopeString,
            exp = tokenEntity.ExpirationDate.HasValue
                ? new DateTimeOffset(tokenEntity.ExpirationDate.Value.ToUniversalTime()).ToUnixTimeSeconds()
                : (long?)null,
            iat = tokenEntity.CreationDate.HasValue
                ? new DateTimeOffset(tokenEntity.CreationDate.Value.ToUniversalTime()).ToUnixTimeSeconds()
                : (long?)null,
            sub = tokenEntity.Subject
        });
    }

    public static IResult UserInfo(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            sub = user.FindFirst(OpenIddictConstants.Claims.Subject)?.Value,
            name = user.FindFirst(OpenIddictConstants.Claims.Name)?.Value,
            role = user.FindFirst(OpenIddictConstants.Claims.Role)?.Value
        });
    }

    public static IResult Revoke(HttpContext context)
    {
        // In production, remove refresh token from DB/cache.
        return Results.Ok(new { revoked = true });
    }

    private static ClaimsPrincipal CreatePrincipal(string username, string role)
    {
        var claims = new List<Claim>
        {
            new(OpenIddictConstants.Claims.Subject, username),
            new(OpenIddictConstants.Claims.Role, role)
        };

        var identity = new ClaimsIdentity(
            claims,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        var principal = new ClaimsPrincipal(identity);
        principal.SetResources("bank_api");
        principal.SetDestinations(claim =>
        {
            if (claim.Type is OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Role)
                return [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken];
            return [OpenIddictConstants.Destinations.AccessToken];
        });

        return principal;
    }
}
