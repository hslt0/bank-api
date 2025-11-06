using Asp.Versioning;

public static partial class ApiMapper
{
    public static WebApplication MapOauthEndpoints(this WebApplication app)
    {
        var oauthItems = app.MapGroup("/connect")
            .WithTags("OAuth")
            .RequireRateLimiting("fixed")
            .RequireCors("generic");
        
        var versionSet = app.NewApiVersionSet("VersionSet")
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        oauthItems.WithApiVersionSet(versionSet);

        // === TOKEN ===
        oauthItems.MapPost("/token", OauthOperation.GetToken)
            .WithName("GetToken")
            .WithSummary("Get access token")
            .WithDescription("Issues an access token using password, client credentials, or refresh token flow.")
            .HasApiVersion(1.0);

        // === AUTHORIZE ===
        oauthItems.MapGet("/authorize", OauthOperation.Authorize)
            .WithName("Authorize")
            .WithSummary("Authorize user")
            .WithDescription("Handles authorization requests and issues authorization codes.")
            .HasApiVersion(1.0);

        // === LOGOUT ===
        oauthItems.MapPost("/logout", OauthOperation.Logout)
            .WithName("Logout")
            .WithSummary("End user session")
            .WithDescription("Revokes tokens and ends the user's session.")
            .HasApiVersion(1.0);

        // === INTROSPECTION ===
        oauthItems.MapPost("/introspect", OauthOperation.Introspect)
            .WithName("Introspect")
            .WithSummary("Introspect token")
            .WithDescription("Validates an access or refresh token and returns metadata about it.")
            .HasApiVersion(1.0);

        // === USERINFO ===
        oauthItems.MapGet("/userinfo", OauthOperation.UserInfo)
            .WithName("UserInfo")
            .WithSummary("User information endpoint")
            .WithDescription("Returns claims about the currently authenticated user.")
            .HasApiVersion(1.0);

        // === REVOKE ===
        oauthItems.MapPost("/revoke", OauthOperation.Revoke)
            .WithName("Revoke")
            .WithSummary("Revoke token")
            .WithDescription("Revokes an existing access or refresh token.")
            .HasApiVersion(1.0);

        return app;
    }
}