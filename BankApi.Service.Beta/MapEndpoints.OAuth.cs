public static partial class ApiMapper
{
    public static WebApplication MapOauthEndpoints(this WebApplication app)
    {
        var oauthItems = app.MapGroup("/connect")
            .WithTags("OAuth")
            .RequireRateLimiting("fixed")
            .RequireCors("generic");

        oauthItems.MapPost("/token", OauthOperation.GetToken)
            .WithName("GetToken")
            .WithSummary("Get token")
            .WithDescription("Return token for a user");
        
        return app;
    }
}