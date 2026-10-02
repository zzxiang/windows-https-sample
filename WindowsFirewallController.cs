using System.Diagnostics;
using System.Security.Principal;

namespace Zhixiang.WindowsHttpsSample;

public static class WindowsFirewallController
{
    public const string RuleName = "WindowsHttpsSample_BlockAllOutbound_9E63D493_51B3_41CE_8E02_2B6C62EA69F4";

    public static string? GetAccessError()
    {
        if (!OperatingSystem.IsWindows())
        {
            return "Windows Firewall controls are available only on Windows.";
        }

        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            if (!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
            {
                return "This action requires Administrator privileges. Close the client and start it from an elevated terminal.";
            }
        }
        catch (Exception exception)
        {
            return $"Unable to check Administrator privileges: {exception.Message}";
        }

        return null;
    }

    public static async Task<string> BlockAllOutboundAsync()
    {
        var accessError = GetAccessError();
        if (accessError is not null)
        {
            return accessError;
        }

        try
        {
            var existingRule = await RunNetshAsync(
                "advfirewall", "firewall", "show", "rule", $"name={RuleName}");
            if (existingRule.Output.Contains(RuleName, StringComparison.OrdinalIgnoreCase))
            {
                return "The sample's outbound block rule already exists. No changes were made.";
            }

            var result = await RunNetshAsync(
                "advfirewall", "firewall", "add", "rule",
                $"name={RuleName}", "dir=out", "action=block", "program=any", "remoteip=any", "profile=any", "enable=yes");

            return result.ExitCode == 0
                ? "The Windows Firewall outbound block rule was added. Outbound network traffic is now blocked machine-wide."
                : FormatFailure("add the outbound block rule", result);
        }
        catch (Exception exception)
        {
            return $"Unable to add the Windows Firewall rule: {exception.Message}";
        }
    }

    public static async Task<string> ResumeOutboundAsync()
    {
        var accessError = GetAccessError();
        if (accessError is not null)
        {
            return accessError;
        }

        try
        {
            var existingRule = await RunNetshAsync(
                "advfirewall", "firewall", "show", "rule", $"name={RuleName}");
            if (!existingRule.Output.Contains(RuleName, StringComparison.OrdinalIgnoreCase))
            {
                return "No matching sample firewall rule was reported. No rule was deleted.";
            }

            var result = await RunNetshAsync(
                "advfirewall", "firewall", "delete", "rule", $"name={RuleName}");

            return result.ExitCode == 0
                ? "The sample's Windows Firewall rule was removed. Existing firewall settings were not changed."
                : FormatFailure("remove the outbound block rule", result);
        }
        catch (Exception exception)
        {
            return $"Unable to remove the Windows Firewall rule: {exception.Message}";
        }
    }

    private static async Task<NetshResult> RunNetshAsync(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, "netsh.exe"),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Windows could not start netsh.exe.");

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new NetshResult(
            process.ExitCode,
            $"{await standardOutput}\n{await standardError}".Trim());
    }

    private static string FormatFailure(string action, NetshResult result)
    {
        var details = string.IsNullOrWhiteSpace(result.Output)
            ? $"netsh exited with code {result.ExitCode}."
            : result.Output;

        return $"Failed to {action}: {details}";
    }

    private sealed record NetshResult(int ExitCode, string Output);
}