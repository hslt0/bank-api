using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
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
                       .AllowAuthorizationCodeFlow()
                       .AllowImplicitFlow()
                       .AllowDeviceAuthorizationFlow();
                
                //TODO: find missing endpoint
                options.SetAuthorizationEndpointUris("v2/connect/authorize");
                options.SetTokenEndpointUris("v2/connect/token");
                options.SetIntrospectionEndpointUris("v2/connect/introspect");
                options.SetRevocationEndpointUris("v2/connect/revoke");
                options.SetUserInfoEndpointUris("v2/connect/userinfo");
                options.SetEndSessionEndpointUris("v2/connect/logout");
                options.SetDeviceAuthorizationEndpointUris("v2/connect/deviceauthorization");
                options.SetEndUserVerificationEndpointUris("v2/connect/enduserverification");

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

        return services;
    }
}