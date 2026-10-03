using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Zhixiang.WindowsHttpsSample;

public static class Program
{
    private static NetworkTracing? _networkTracing;

    public const int Port = 5001;
    public const int TimeoutSeconds = 5;
    public static IPAddress SelectedAddress { get; private set; } = IPAddress.Loopback;
    public static string BaseUrl { get; private set; } = $"https://{FormatHostForUrl(IPAddress.Loopback)}:{Port}";

    public static void Log(string message)
    {
        var line = $"[{DateTimeOffset.UtcNow:O}] {message}";
        Console.WriteLine(line);
        _networkTracing?.WriteApplicationLog(line);
    }

    public static async Task Main(string[] args)
    {
        if (args.Length == 0 ||
            (!string.Equals(args[0], "server", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(args[0], "client", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- server");
            Console.WriteLine("  dotnet run -- client");
            return;
        }

        using var networkTracing = new NetworkTracing(args[0]);
        _networkTracing = networkTracing;
        Console.WriteLine($"Network trace and application log: {networkTracing.LogFilePath}");

        SelectedAddress = SelectAvailableAddress();
        BaseUrl = $"https://{FormatHostForUrl(SelectedAddress)}:{Port}";

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
    }

    private static IPAddress SelectAvailableAddress()
    {
        var addresses = GetAvailableAddresses();
        if (addresses.Count == 0)
        {
            return IPAddress.Loopback;
        }

        Console.WriteLine("Available local IP addresses:");
        for (var index = 0; index < addresses.Count; index++)
        {
            Console.WriteLine($"  {index + 1}. {addresses[index]}");
        }

        Console.WriteLine($"  0. Custom IP address");

        while (true)
        {
            Console.Write($"Select an IP address (1-{addresses.Count}) or type 0 for custom: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (int.TryParse(input, out var selection))
            {
                if (selection == 0)
                {
                    return ParseCustomAddress();
                }

                if (selection >= 1 && selection <= addresses.Count)
                {
                    return addresses[selection - 1];
                }
            }

            if (IPAddress.TryParse(input, out var parsedAddress))
            {
                return parsedAddress;
            }

            Console.WriteLine("Invalid selection. Enter a listed number or a valid IPv4/IPv6 address.");
        }
    }

    private static List<IPAddress> GetAvailableAddresses()
    {
        return NetworkInterface
            .GetAllNetworkInterfaces()
            .SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
            .Select(unicastAddress => unicastAddress.Address)
            .Where(address => address is not null)
            .Where(address => address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
            .Where(address => !IPAddress.IsLoopback(address))
            .Where(address => !(address.AddressFamily == AddressFamily.InterNetworkV6 && address.IsIPv6LinkLocal))
            .Concat([IPAddress.Loopback, IPAddress.IPv6Loopback])
            .Select(address => address.ToString())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(IPAddress.Parse)
            .OrderBy(address => address.AddressFamily == AddressFamily.InterNetworkV6 ? 1 : 0)
            .ThenBy(address => address.ToString(), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IPAddress ParseCustomAddress()
    {
        while (true)
        {
            Console.Write("Enter a custom IP address: ");
            var input = Console.ReadLine()?.Trim();
            if (IPAddress.TryParse(input, out var parsedAddress))
            {
                return parsedAddress;
            }

            Console.WriteLine("The value entered is not a valid IPv4 or IPv6 address. Please try again.");
        }
    }

    private static string FormatHostForUrl(IPAddress address)
    {
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return $"[{address}]";
        }

        return address.ToString();
    }
}
