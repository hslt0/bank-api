using BankApi.Client.Services;

Console.WriteLine("=== BankApi OAuth Client ===");
Console.Write("Enter API base URL (e.g. https://localhost:5202): ");
var baseUrl = "https://localhost:5202";

var auth = new AuthClient(baseUrl);
var device = new DeviceFlowClient(baseUrl);

while (true)
{
    Console.WriteLine("""
    
    Choose action:
    1. Password grant
    2. Client credentials grant
    3. User info (requires token)
    4. Introspect token
    5. Revoke token
    6. Device authorization flow
    0. Exit
    """);

    Console.Write("> ");
    var key = Console.ReadLine();

    switch (key)
    {
        case "1":
            Console.Write("Username: ");
            var username = Console.ReadLine()!;
            Console.Write("Password: ");
            var password = Console.ReadLine()!;
            var token = await auth.PasswordGrantAsync(username, password);
            Console.WriteLine(token);
            break;

        case "2":
            Console.Write("Client ID: ");
            var clientId = Console.ReadLine()!;
            Console.Write("Client Secret: ");
            var clientSecret = Console.ReadLine()!;
            var clientToken = await auth.ClientCredentialsGrantAsync(clientId, clientSecret);
            Console.WriteLine(clientToken);
            break;

        case "3":
            Console.Write("Access token: ");
            var access = Console.ReadLine()!;
            await auth.UserInfoAsync(access);
            break;

        case "4":
            Console.Write("Token: ");
            var introspect = Console.ReadLine()!;
            await auth.IntrospectAsync(introspect);
            break;

        case "5":
            Console.Write("Token to revoke: ");
            var revoke = Console.ReadLine()!;
            await auth.RevokeAsync(revoke);
            break;

        case "6":
            Console.Write("Client ID: ");
            var cId = Console.ReadLine()!;
            var (deviceCode, userCode, verificationUri) = await device.StartDeviceFlowAsync(cId);
            Console.WriteLine($"Now go to {verificationUri} and enter user code {userCode}");

            Console.Write("Enter username: ");
            var u = Console.ReadLine()!;
            Console.Write("Enter password: ");
            var p = Console.ReadLine()!;
            await device.VerifyAsync(userCode, u, p);
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Unknown option.");
            break;
    }
}
