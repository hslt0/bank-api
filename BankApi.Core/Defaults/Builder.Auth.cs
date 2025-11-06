using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

public static partial class ApiBuilder
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<OAuthDb>();
            })
            .AddServer(options =>
            {
                options.AllowPasswordFlow()
                       .AllowClientCredentialsFlow()
                       .AllowRefreshTokenFlow()
                       .AllowAuthorizationCodeFlow();
                
                options.SetAuthorizationEndpointUris("connect/authorize");
                options.SetTokenEndpointUris("connect/token");
                options.SetIntrospectionEndpointUris("connect/introspect");
                options.SetRevocationEndpointUris("connect/revoke");
                options.SetUserInfoEndpointUris("connect/userinfo");
                options.SetEndSessionEndpointUris("connect/logout");

                options.AcceptAnonymousClients();

                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableEndSessionEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough()
                       .EnableStatusCodePagesIntegration();
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
                    context.ValidationSucceeded();
                else
                    context.ValidationFailed();

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

        services.AddHostedService<OAuthSeedHostedService>();

        return services;
    }
}

public class OAuthSeedHostedService(IServiceProvider provider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("console-client", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "console-client",
                ClientSecret = "console-secret",
                DisplayName = "Console Client App",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.DeviceCode,
                    OpenIddictConstants.Permissions.Scopes.Profile
                }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}