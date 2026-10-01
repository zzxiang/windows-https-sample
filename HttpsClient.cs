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
            Console.WriteLine("3. Quit");
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
                    Console.WriteLine("Exiting the HTTPS client.");
                    return;
                default:
                    Console.WriteLine("Invalid selection. Please choose 1, 2, or 3.");
                    break;
            }
        }
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
