using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public static partial class ApiBuilder
{
    private static readonly InMemoryDatabaseRoot OAuthDatabaseRoot = new();
    
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddDbContext<BankDb>(options =>
        {
            options.UseInMemoryDatabase(GlobalConfiguration.ApiSettings!.DatabaseName);
        });
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = GlobalConfiguration.ApiSettings!.Cache;
        });
        services.AddDatabaseDeveloperPageExceptionFilter();
        
        services.AddDbContext<OAuthDb>(options =>
            options.UseInMemoryDatabase(GlobalConfiguration.ApiSettings!.OAuthDatabaseName, OAuthDatabaseRoot));

        return services;
    }

    public static void EnsureDataServicesCreated(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<BankDb>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<OAuthDb>().Database.EnsureCreated();
    }
}