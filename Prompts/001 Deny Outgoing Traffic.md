# Deny Outgoing Traffic

Add a menu option in the client to block all the outgoing traffic on the machine. This feature may needs OS firewall controls, so it is OK to require administrator priviliges, but the user needs to be informed.

Also add another menu option to reset resume the traffic.

I know this is a risky feature. Since this is just a sample, we don't need to consider too much about security issues.

## Plan: Machine-Wide Outbound Traffic Toggle

Add two actions to the interactive HTTPS client that block machine-wide outbound traffic using one uniquely named Windows Firewall rule, then resume traffic by deleting only that rule. The client must be launched as Administrator; it will explain the machine-wide impact, require confirmation before blocking, and report failures clearly. The firewall rule persists after the client exits until resumed.

### Steps

1. Add a small Windows Firewall controller, using the existing project style, to check for Windows/admin context and invoke `netsh advfirewall` with fixed arguments through `ProcessStartInfo.ArgumentList`. Add one stable, app-specific outbound block rule for all programs, all remote IPs, and all profiles. Resume deletes only that exact rule; do not change profile defaults or unrelated rules.
2. Extend `HttpsClient.RunAsync` with separate block and resume menu actions. Before adding the rule, display the machine-wide and persistent-impact warning and require explicit confirmation. Require an already elevated process and explain how to restart as Administrator when not elevated. Handle cancellation, duplicate/no-op states, command errors, and process launch errors without terminating the menu.
3. Update `README.md` with the all-profiles scope, Administrator requirement, persistent-until-resumed behavior, and recovery steps if the client closes or the user needs to remove the named rule manually.
4. Build the project and manually validate on Windows in an elevated environment: add the rule and verify its exact name/scope, verify outbound traffic is blocked, remove it through the menu, and verify traffic resumes. Also test decline, non-elevated use, repeated block/resume, and command failure reporting. Do not run the destructive firewall validation on an uncontrolled machine.

### Relevant files

- `HttpsClient.cs` — menu and user-facing warning/confirmation.
- `WindowsFirewallController.cs` — new focused owner for elevation checks and exact add/remove firewall commands.
- `README.md` — document scope, persistence, privilege requirement, and recovery.
- `windows-https-sample.csproj` — likely unchanged; confirm Windows targeting/platform assumptions during implementation.

### Verification

1. Run `dotnet build` for the project.
2. On a disposable/controlled Windows test machine, use an elevated console to verify the app-owned firewall rule appears with outbound/block/all-profile scope, blocks an external connection, and is removed by resume so the connection works again.
3. Verify non-elevated actions fail safely with actionable text; declining confirmation makes no firewall change; repeated block/resume operations are idempotent; app-owned cleanup does not delete unrelated rules.
4. Review README recovery instructions against the actual rule name and exact remove command.

### Decisions

- Machine-wide scope across Windows Firewall profiles, implemented as a distinct app-owned block rule, not by changing profile default policy.
- Resume removes only the sample's rule; it does not restore or overwrite pre-existing firewall configuration.
- The client must already be running as Administrator; no UAC relaunch/helper flow.
- The block is persistent until explicitly resumed, including after client exit or reboot. Warn and confirm before enabling.
- The feature controls Windows Defender Firewall. It cannot promise to override a disabled firewall or other host/network controls; document that limitation.
