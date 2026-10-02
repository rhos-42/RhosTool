using System.Collections.Concurrent;

namespace RhosTool.Core.Services;

public sealed class LogService
{
    private readonly ConcurrentQueue<string> _lines = new();
    private const int MaxLines = 500;

    public event Action<string>? LineAdded;

    public IReadOnlyList<string> Snapshot() => _lines.ToArray();

    public void Info(string message) => Append("INFO", message);

    public void Warn(string message) => Append("WARN", message);

    public void Error(string message) => Append("ERROR", message);

    public void Success(string message) => Append("OK", message);

    private void Append(string level, string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        _lines.Enqueue(line);

        while (_lines.Count > MaxLines && _lines.TryDequeue(out _))
        {
        }

        LineAdded?.Invoke(line);
    }
}
