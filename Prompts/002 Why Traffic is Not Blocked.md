# Why Traffic is Not Blocked?

Me>

The firewall rule does not block traffic to the sample server running on the same machine. Why?

Copilot (GPT-6 Luna)>

I’ll trace how the firewall rule is created and how the sample server is bound, since local loopback traffic can be treated differently from ordinary outbound traffic. Then I’ll explain the cause and the smallest reliable adjustment.

Read memory

The local path is confirmed: the server listens on IPv6 loopback with dual mode enabled, while the client targets `localhost`. I’m checking the URL configuration next so I can distinguish an actual rule-construction bug from Windows’ loopback behavior and give the right test setup.

Answer:

This is loopback traffic, not normal outbound network traffic. The client targets `https://localhost:5001` in `Program.cs`, and `HttpsServer.cs` binds the listener to `IPv6Loopback` (with dual mode enabled). The firewall rule in `WindowsFirewallController.cs` blocks outbound traffic, but Windows treats localhost connections specially; that rule is not a reliable way to block a client talking to a server on the same machine.

For a reliable firewall demo, run the server on a different machine or VM, bind it to a reachable network interface, and point the client’s `BaseUrl` at that host’s address. The existing outbound rule should then block the client’s connection. Simply changing `localhost` to the machine’s own LAN address may still use a local route, so a separate host is the dependable test.

If both processes must stay on the same machine, use an application-level test (for example, stop the server or reject requests) rather than Windows Firewall to simulate the interruption.
