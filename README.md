# Windows HTTPS Sample

This project demonstrates a simple HTTPS server and client using a self-signed certificate in .NET 10.

## Features

- HTTPS server hosted on the selected local IP address at port `5001` (for example `https://192.168.1.9:5001`)
- Startup prompt that lists available IP addresses and allows a custom IP entry
- Self-signed certificate generated automatically at runtime for the selected address and localhost loopback values
- Responsive API: returns a text message
- Timeout API: accepts the request but delays without responding so the client can observe a timeout
- Interactive console client that waits for a user selection and prints timestamps for request, response, and timeout errors
- Optional machine-wide outbound traffic block and resume actions through Windows Firewall

## Source Code

- `Program.cs` - entry point that starts either the server or the client
- `HttpsServer.cs` - HTTPS server using Kestrel
- `HttpsClient.cs` - interactive client that calls the APIs
- `SelfSignedCertificate.cs` - creates and loads a local self-signed certificate

## Requirements

- .NET 10 SDK
- Windows environment for the sample flow

## Run the sample

Open two terminal windows in the project root.

### 1. Start the HTTPS server

```bash
dotnet run -- server
```

### 2. Start the HTTPS client

```bash
dotnet run -- client
```

Run the client from an Administrator terminal to use the firewall actions. The other client actions do not require elevation.

The client will prompt for a choice:

- `1` - call the responsive API
- `2` - call the timeout API
- `3` - block outbound network traffic machine-wide (requires confirmation and Administrator privileges)
- `4` - remove the sample's outbound block rule
- `5` - exit

The block action adds a Windows Defender Firewall outbound block rule for all programs, remote addresses, and profiles. It can interrupt all network access and remains active after the client exits or the machine restarts. Select `4` to resume traffic. The resume action removes only the sample's rule and does not modify other firewall rules or default policies.

If the client is unavailable, open an Administrator terminal and remove the sample's rule with:

```powershell
netsh advfirewall firewall delete rule name=WindowsHttpsSample_BlockAllOutbound_9E63D493_51B3_41CE_8E02_2B6C62EA69F4
```

These actions control Windows Defender Firewall only. They cannot guarantee that traffic is blocked if the firewall is disabled or overridden by other system or network controls.

## API endpoints

- `GET https://localhost:5001/api/responsive`  
  Returns a small text response.

- `GET https://localhost:5001/api/timeout`  
  Accepts the request and waits without returning a response, which causes the client timeout to trigger.

## Notes

The self-signed certificate is generated automatically and stored under the application output directory in a `certs` folder. Because the certificate is self-signed, the client disables certificate validation for the sample so it can connect successfully.

This sample is intended for learning and local testing only.
