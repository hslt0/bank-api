using OpenIddict.Validation.AspNetCore;

namespace BankApi.Core.Defaults;

public static partial class ApiBuilder
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(_ =>
            {
                //There is must be connected DB
                //For now this is in memory
            })
            .AddServer(options =>
            {
                options.AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                options.SetTokenEndpointUris("/v2/connect/token");

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

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
        
        services.AddAuthorization();
        
        return services;
    }
}