# Windows HTTPS Sample

This project demonstrates a simple HTTPS server and client using a self-signed certificate in .NET 10.

## Features

- HTTPS server hosted on `https://localhost:5001`
- Self-signed certificate generated automatically at runtime
- Responsive API: returns a text message
- Timeout API: accepts the request but delays without responding so the client can observe a timeout
- Interactive console client that waits for a user selection and prints timestamps for request, response, and timeout errors

## Project layout

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

The client will prompt for a choice:

- `1` - call the responsive API
- `2` - call the timeout API
- `3` - exit

## API endpoints

- `GET https://localhost:5001/api/responsive`  
  Returns a small text response.

- `GET https://localhost:5001/api/timeout`  
  Accepts the request and waits without returning a response, which causes the client timeout to trigger.

## Notes

The self-signed certificate is generated automatically and stored under the application output directory in a `certs` folder. Because the certificate is self-signed, the client disables certificate validation for the sample so it can connect successfully.

This sample is intended for learning and local testing only.
