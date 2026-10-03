using System.Diagnostics.Tracing;

namespace Zhixiang.WindowsHttpsSample;

public sealed class NetworkTracing : EventListener
{
    private readonly object _writeLock = new();
    private StreamWriter? _writer;

    public NetworkTracing(string role)
    {
        var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logDirectory);
        LogFilePath = Path.Combine(logDirectory, $"{role.ToLowerInvariant()}.log");
        _writer = new StreamWriter(
            new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };

        foreach (var eventSource in EventSource.GetSources())
        {
            EnableNetworkEvents(eventSource);
        }
    }

    public string LogFilePath { get; }

    public void WriteApplicationLog(string line)
    {
        WriteLineToFile(line);
    }

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        EnableNetworkEvents(eventSource);
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        var payload = string.Empty;
        if (eventData.Payload is not null)
        {
            var values = new List<string>(eventData.Payload.Count);
            for (var index = 0; index < eventData.Payload.Count; index++)
            {
                var name = eventData.PayloadNames is not null &&
                           index < eventData.PayloadNames.Count
                    ? eventData.PayloadNames[index]
                    : $"arg{index}";
                values.Add($"{name}={eventData.Payload[index]}");
            }

            if (values.Count > 0)
            {
                payload = $" | {string.Join(", ", values)}";
            }
        }

        var eventName = eventData.EventName ?? $"Event{eventData.EventId}";
        WriteLineToFile(
            $"[{DateTimeOffset.UtcNow:O}] [NET {eventData.EventSource.Name}/{eventName}]{payload}");
    }

    public override void Dispose()
    {
        base.Dispose();
        lock (_writeLock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }

    private void EnableNetworkEvents(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith("System.Net.", StringComparison.Ordinal))
        {
            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        }
    }

    private void WriteLineToFile(string line)
    {
        lock (_writeLock)
        {
            _writer?.WriteLine(line);
        }
    }
}
