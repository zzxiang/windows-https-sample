namespace Zhixiang.WindowsHttpsSample;

public static class Program
{
    public const string BaseUrl = "https://localhost:5001";
    public const int TimeoutSeconds = 5;

    public static void Log(string message)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] {message}");
    }

    public static async Task Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "server", StringComparison.OrdinalIgnoreCase))
        {
            await HttpsServer.RunAsync();
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "client", StringComparison.OrdinalIgnoreCase))
        {
            await HttpsClient.RunAsync();
            return;
        }

        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- server");
        Console.WriteLine("  dotnet run -- client");
    }
}
