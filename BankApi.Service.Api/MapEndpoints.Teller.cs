using Asp.Versioning;

public static partial class ApiMapper
{
    public static WebApplication MapTellerEndpoints(this WebApplication app)
    {
        var tellerItems = app.MapGroup("/teller")
            .WithTags("Administrative")
            .RequireRateLimiting("fixed")
            .RequireAuthorization("bank_god")
            .RequireCors("generic");
        
        var versionSet = app.NewApiVersionSet("VersionSet")
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        tellerItems.WithApiVersionSet(versionSet);

        tellerItems.MapGet("/", TellerOperation.GetBankTeller)
            .WithName("GetBankTeller").WithSummary("Get bank teller")
            .WithDescription("Get the teller of all banks.")
            .HasApiVersion(1.0);

        tellerItems.MapGet("/reports", TellerOperation.GetBankTellerReports)
            .WithName("GetBankTellerReports").WithSummary("Get bank teller reports")
            .WithDescription("Get the modern teller reports.")
            .HasApiVersion(1.0);

        return app;
    }
}