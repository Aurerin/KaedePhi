using System;
using System.IO;

namespace KaedePhi.Tool.Gui.Services;

public sealed class LogService
{
    private readonly string _logDir;
    private readonly object _lock = new();

    public LogService()
    {
        _logDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(_logDir);
    }

    public string LogDirectory => _logDir;

    public string CurrentLogFile { get; private set; } = string.Empty;

    public void StartSession()
    {
        var fileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        CurrentLogFile = Path.Combine(_logDir, fileName);
        WriteToFile("=== KaedePhi GUI Session Started ===");
    }

    public void Info(string message)
    {
        WriteToFile($"[INFO  {DateTime.Now:HH:mm:ss}] {message}");
    }

    public void Warn(string message)
    {
        WriteToFile($"[WARN  {DateTime.Now:HH:mm:ss}] {message}");
    }

    public void Error(string message, Exception? ex = null)
    {
        var line = $"[ERROR {DateTime.Now:HH:mm:ss}] {message}";
        if (ex != null)
            line += $"\n  Exception: {ex.GetType().Name}: {ex.Message}\n  StackTrace: {ex.StackTrace}";
        WriteToFile(line);
    }

    public void Step(string stepName)
    {
        WriteToFile($"[STEP  {DateTime.Now:HH:mm:ss}] --- {stepName} ---");
    }

    private void WriteToFile(string line)
    {
        lock (_lock)
        {
            try
            {
                File.AppendAllText(CurrentLogFile, line + Environment.NewLine);
            }
            catch
            {
                // Logging failure should not crash the application
            }
        }
    }
}
