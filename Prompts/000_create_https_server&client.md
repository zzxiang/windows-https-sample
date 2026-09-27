Create a C# project implementing a HTTPS server and a HTTPS client.

Use the latest stable .NET version.

The server uses a self-signed certificate and exposes the following APIs to the client.

- Responsive API: response the client with a text message
- Timeout API: receive the request from client but do not give any response to make the request timeout

The client wait for keyboard input from user. The user can choose to execute any of the API
exposed by the server, or quit.

The client prints the timestamps of request, response and timeout error.

All the source code should be in the `Zhixiang.WindowsHttpsSample` namespace.

Generate a README for the project.
