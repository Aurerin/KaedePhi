using System;
using System.IO;
using System.Linq;

namespace KaedePhi.Tool.Gui.Services;

public sealed class LogService
{
    private readonly string _logDir;
    private readonly int _maxLogFiles;
    private readonly object _lock = new();

    public LogService(int maxLogFiles = 5)
    {
        _logDir = Path.Combine(AppContext.BaseDirectory, "logs");
        _maxLogFiles = maxLogFiles;
        Directory.CreateDirectory(_logDir);
    }

    public string LogDirectory => _logDir;

    public string CurrentLogFile { get; private set; } = string.Empty;

    public void StartSession()
    {
        var fileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        CurrentLogFile = Path.Combine(_logDir, fileName);
        WriteToFile("=== KaedePhi GUI Session Started ===");
        CleanupOldLogs();
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

    private void CleanupOldLogs()
    {
        try
        {
            var files = new DirectoryInfo(_logDir)
                .GetFiles("session_*.log")
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            for (var i = _maxLogFiles; i < files.Count; i++)
                files[i].Delete();
        }
        catch
        {
            // Cleanup failure should not crash the application
        }
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
