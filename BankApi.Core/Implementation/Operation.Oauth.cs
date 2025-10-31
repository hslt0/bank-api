using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

public class OauthOperation
{
    public static IResult GetToken(HttpContext context)
    {
        var request = context.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("Cannot get OpenIddict request.");

        if (!request.IsPasswordGrantType()) 
            return Results.BadRequest(new { error = "Unsupported grant type." });
            
        if (request.Username != "admin" || request.Password != "1234")
        {
            return Results.Forbid(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid username or password."
                }));
        }

        var claims = new List<Claim>
        {
            new(OpenIddictConstants.Claims.Subject, request.Username!),
            new(OpenIddictConstants.Claims.Name, "Administrator"),
            // Add any roles if needed
            // new(OpenIddictConstants.Claims.Role, "banker")
        };

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        var principal = new ClaimsPrincipal(identity);

        // Set scopes if needed
        principal.SetScopes(new[]
        {
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile
            // Add custom scopes if needed
        });

        // This will return a proper OAuth token response
        return Results.SignIn(principal, 
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}