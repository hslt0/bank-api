using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

public static partial class ApiMapper
{
    public static WebApplication MapBankEndpoints(this WebApplication app)
    {
        var bankItems = app.MapGroup("/banks")
            .WithTags("Supervisory")
            .RequireRateLimiting("fixed")
            .RequireAuthorization("bank_subscription")
            .RequireCors("generic");

        bankItems.MapGet("/", BankOperation.GetAllBanks)
            .WithName("GetAllBanks").WithSummary("Get all banks")
            .WithDescription("Get all banks in the Netherlands.");

        bankItems.MapGet("/{id}", BankOperation.GetBank)
            .WithName("GetBank").WithSummary("Get a bank")
            .WithDescription("Get a bank in the Netherlands.");

        bankItems.MapPost("/", BankOperation.CreateBank)
            .WithName("CreateBank").WithSummary("Create a bank")
            .WithDescription("Create a bank in the Netherlands.");

        bankItems.MapPut("/{id}", BankOperation.UpdateBank)
            .WithName("UpdateBank").WithSummary("Update a bank")
            .WithDescription("Update a bank in the Netherlands.");

        bankItems.MapDelete("/{id}", BankOperation.DeleteBank)
            .WithName("DeleteBank").WithSummary("Delete a bank")
            .WithDescription("Delete a bank in the Netherlands.");

        app.MapPost("/connect/token", (HttpContext context) =>
        {
            var request = context.GetOpenIddictServerRequest()
                          ?? throw new InvalidOperationException("Cannot get OpenIddict request.");

            if (!request.IsPasswordGrantType()) return Results.BadRequest(new { error = "Unsupported grant type." });
            
            if (request.Username != "admin" || request.Password != "1234")
                return Results.Unauthorized();

            var claims = new List<Claim>
            {
                new(OpenIddictConstants.Claims.Subject, request.Username!),
                new(OpenIddictConstants.Claims.Name, "Administrator")
            };

            var identity = new ClaimsIdentity(
                claims,
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);

            var principal = new ClaimsPrincipal(identity);

            return Results.SignIn(principal, properties: null,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        });
        
        return app;
    }
}