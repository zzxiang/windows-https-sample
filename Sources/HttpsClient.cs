namespace Zhixiang.WindowsHttpsSample;

public static class HttpsClient
{
    public static async Task RunAsync()
    {
        using var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        {
            BaseAddress = new Uri(Program.BaseUrl),
            Timeout = TimeSpan.FromSeconds(Program.TimeoutSeconds)
        };

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Choose an HTTPS API:");
            Console.WriteLine("1. Responsive API");
            Console.WriteLine("2. Timeout API");
            Console.WriteLine("3. Block all outgoing traffic on this machine");
            Console.WriteLine("4. Resume outgoing traffic");
            Console.WriteLine("5. Quit");
            Console.Write("Selection: ");

            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            switch (input.Trim())
            {
                case "1":
                    await InvokeResponsiveAsync(client);
                    break;
                case "2":
                    await InvokeTimeoutAsync(client);
                    break;
                case "3":
                    await BlockOutgoingTrafficAsync();
                    break;
                case "4":
                    await ResumeOutgoingTrafficAsync();
                    break;
                case "5":
                    Console.WriteLine("Exiting the HTTPS client.");
                    return;
                default:
                    Console.WriteLine("Invalid selection. Please choose 1, 2, 3, 4, or 5.");
                    break;
            }
        }
    }

    private static async Task BlockOutgoingTrafficAsync()
    {
        var accessError = WindowsFirewallController.GetAccessError();
        if (accessError is not null)
        {
            Console.WriteLine(accessError);
            return;
        }

        Console.WriteLine("WARNING: This adds a Windows Firewall rule that blocks outbound network traffic for all applications and profiles on this machine.");
        Console.WriteLine("The rule remains active after this client exits and can interrupt network access. Use menu option 4 to remove it.");
        Console.Write("Type BLOCK to confirm: ");

        if (!string.Equals(Console.ReadLine()?.Trim(), "BLOCK", StringComparison.Ordinal))
        {
            Console.WriteLine("Cancelled. No firewall changes were made.");
            return;
        }

        Console.WriteLine(await WindowsFirewallController.BlockAllOutboundAsync());
    }

    private static async Task ResumeOutgoingTrafficAsync()
    {
        Console.WriteLine(await WindowsFirewallController.ResumeOutboundAsync());
    }

    private static async Task InvokeResponsiveAsync(HttpClient client)
    {
        Program.Log("Request sent to /api/responsive.");

        try
        {
            using var response = await client.GetAsync("/api/responsive");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Program.Log($"Response received from /api/responsive: {responseBody}");
        }
        catch (Exception exception)
        {
            Program.Log($"Error calling /api/responsive: {exception.Message}");
        }
    }

    private static async Task InvokeTimeoutAsync(HttpClient client)
    {
        Program.Log("Request sent to /api/timeout.");

        try
        {
            using var response = await client.GetAsync("/api/timeout");
            Program.Log($"Unexpected response received from /api/timeout. Status: {response.StatusCode}");
        }
        catch (TaskCanceledException)
        {
            Program.Log($"Timeout error: the request exceeded {Program.TimeoutSeconds} seconds.");
        }
        catch (Exception exception)
        {
            Program.Log($"Error calling /api/timeout: {exception.Message}");
        }
    }
}
