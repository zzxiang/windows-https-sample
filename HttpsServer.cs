using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Zhixiang.WindowsHttpsSample;

public static class HttpsServer
{
    public static async Task RunAsync()
    {
        var certificate = SelfSignedCertificate.EnsureCertificate();
        var listener = new TcpListener(IPAddress.IPv6Loopback, 5001);
        listener.Server.DualMode = true;
        listener.Start();

        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] HTTPS server is listening on {Program.BaseUrl}");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(async () =>
            {
                using var connectedClient = client;
                try
                {
                    await HandleClientAsync(connectedClient, certificate);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] HTTPS request failed: {exception.Message}");
                }
            });
        }
    }

    private static async Task HandleClientAsync(TcpClient client, X509Certificate2 certificate)
    {
        using var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);
        await sslStream.AuthenticateAsServerAsync(certificate, clientCertificateRequired: false, enabledSslProtocols: SslProtocols.Tls12 | SslProtocols.Tls13, checkCertificateRevocation: false);

        var requestText = await ReadRequestAsync(sslStream);
        var requestLine = requestText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
        var segments = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var path = segments.Length > 1 ? segments[1] : "/";

        if (path.Equals("/api/responsive", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Responsive API received a request.");
            var message = $"Hello from the HTTPS server. Time: {DateTimeOffset.UtcNow:O}";
            await WriteResponseAsync(sslStream, 200, message);
            return;
        }

        if (path.Equals("/api/timeout", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Timeout API received a request and is delaying without a response.");
            await Task.Delay(TimeSpan.FromMinutes(2));
            return;
        }

        await WriteResponseAsync(sslStream, 404, "Not found.");
    }

    private static async Task<string> ReadRequestAsync(SslStream sslStream)
    {
        var requestBuilder = new StringBuilder();
        var buffer = new char[1];
        using var reader = new StreamReader(sslStream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

        while (true)
        {
            var count = await reader.ReadBlockAsync(buffer, 0, 1);
            if (count == 0)
            {
                break;
            }

            requestBuilder.Append(buffer[0]);
            if (requestBuilder.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
            {
                break;
            }
        }

        return requestBuilder.ToString();
    }

    private static async Task WriteResponseAsync(SslStream sslStream, int statusCode, string message)
    {
        var responseBody = Encoding.UTF8.GetBytes(message);
        var response = $"HTTP/1.1 {statusCode} {(statusCode == 200 ? "OK" : "Not Found")}\r\n" +
                       $"Content-Type: text/plain; charset=utf-8\r\n" +
                       $"Content-Length: {responseBody.Length}\r\n" +
                       "Connection: close\r\n\r\n";

        var responseBytes = Encoding.UTF8.GetBytes(response);
        var payload = new byte[responseBytes.Length + responseBody.Length];
        Buffer.BlockCopy(responseBytes, 0, payload, 0, responseBytes.Length);
        Buffer.BlockCopy(responseBody, 0, payload, responseBytes.Length, responseBody.Length);

        await sslStream.WriteAsync(payload);
        await sslStream.FlushAsync();
    }
}
