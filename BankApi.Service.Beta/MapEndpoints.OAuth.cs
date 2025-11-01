public static partial class ApiMapper
{
    public static WebApplication MapOauthEndpoints(this WebApplication app)
    {
        var oauthItems = app.MapGroup("/connect")
            .WithTags("OAuth")
            .RequireRateLimiting("fixed")
            .RequireCors("generic");

        // === TOKEN ===
        oauthItems.MapPost("/token", OauthOperation.GetToken)
            .WithName("GetToken")
            .WithSummary("Get access token")
            .WithDescription("Issues an access token using password, client credentials, or refresh token flow.");

        // === AUTHORIZE ===
        oauthItems.MapGet("/authorize", OauthOperation.Authorize)
            .WithName("Authorize")
            .WithSummary("Authorize user")
            .WithDescription("Handles authorization requests and issues authorization codes.");

        // === LOGOUT ===
        oauthItems.MapPost("/logout", OauthOperation.Logout)
            .WithName("Logout")
            .WithSummary("End user session")
            .WithDescription("Revokes tokens and ends the user's session.");

        // === INTROSPECTION ===
        oauthItems.MapPost("/introspect", OauthOperation.Introspect)
            .WithName("Introspect")
            .WithSummary("Introspect token")
            .WithDescription("Validates an access or refresh token and returns metadata about it.");

        // === USERINFO ===
        oauthItems.MapGet("/userinfo", OauthOperation.UserInfo)
            .WithName("UserInfo")
            .WithSummary("User information endpoint")
            .WithDescription("Returns claims about the currently authenticated user.");

        // === REVOKE ===
        oauthItems.MapPost("/revoke", OauthOperation.Revoke)
            .WithName("Revoke")
            .WithSummary("Revoke token")
            .WithDescription("Revokes an existing access or refresh token.");

        return app;
    }
}