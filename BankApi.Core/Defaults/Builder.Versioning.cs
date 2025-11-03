using Asp.Versioning;

namespace BankApi.Core.Defaults;

public static partial class ApiMapper
{
    public static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
        });

        services.AddEndpointsApiExplorer();
        
        return services;
    }
}