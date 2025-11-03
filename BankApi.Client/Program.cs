using BankApi.Client;

var http = new HttpClient
{
    BaseAddress = new Uri("https://localhost:5202")
};

var api = new ApiClient(http);

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input))
        continue;

    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var cmd = parts[0].ToLower();

    try
    {
        switch (cmd)
        {
            case "exit":
                return;

            case "login":
                Console.Write("Username: ");
                var username = Console.ReadLine()!;
                Console.Write("Password: ");
                var password = ReadPassword();
                await api.LoginAsync(username, password);
                break;

            case "banks":
                await api.GetBanksAsync();
                break;

            default:
                Console.WriteLine("Commands: login, banks, exit");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

static string ReadPassword()
{
    var pass = string.Empty;
    ConsoleKey key;
    do
    {
        var keyInfo = Console.ReadKey(intercept: true);
        key = keyInfo.Key;

        if (key == ConsoleKey.Backspace && pass.Length > 0)
        {
            pass = pass[..^1];
            Console.Write("\b \b");
        }
        else if (!char.IsControl(keyInfo.KeyChar))
        {
            pass += keyInfo.KeyChar;
            Console.Write("*");
        }
    } while (key != ConsoleKey.Enter);
    Console.WriteLine();
    return pass;
}