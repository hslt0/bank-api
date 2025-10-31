using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

public static partial class ApiBuilder
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(_ =>
            {
                //Todo: add db, probably must start working
            })
            .AddServer(options =>
            {
                options.AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();
                options.SetTokenEndpointUris("/v1/connect/token", "/v2/connect/token");
                options.AcceptAnonymousClients();
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        ApiKeyEvents apiKeyEvents = new()
        {
            OnValidateKey = context =>
            {
                if (context.ApiKey == "Lifetime Subscription")
                {
                    context.ValidationSucceeded();
                }
                else
                {
                    context.ValidationFailed();
                }
                return Task.CompletedTask;
            }
        };

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            })
            .AddApiKeyInHeader($"{ApiKeyDefaults.AuthenticationScheme}-Header", options =>
            {
                options.KeyName = "Ocp-Apim-Subscription-Key";
                options.Realm = "API";
                options.Events = apiKeyEvents;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = GlobalConfiguration.ApiSettings!.TokenValidation;
                options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("bank_god", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole("banker", "ceo");
            });
            options.AddPolicy("bank_subscription", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add($"{ApiKeyDefaults.AuthenticationScheme}-Header");
            });
        });

        return services;
    }
}