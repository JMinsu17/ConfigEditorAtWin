using System;
using System.IO;

namespace ConfigEditor.Infrastructure;

/// <summary>
/// Thread-safe local logging service writing to %LocalAppData%/ConfigEditor/Logs/.
/// </summary>
public class LogService
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public LogService()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string logDir = Path.Combine(localAppData, "ConfigEditor", "Logs");
        
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        _logFilePath = Path.Combine(logDir, "config_editor.log");
    }

    public void LogInfo(string message) => Log("INFO", message);
    public void LogWarning(string message) => Log("WARN", message);
    public void LogError(string message, Exception? ex = null)
    {
        string formatted = message;
        if (ex != null)
        {
            formatted += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
        }
        Log("ERROR", formatted);
    }

    private void Log(string level, string message)
    {
        try
        {
            lock (_lock)
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
        }
        catch
        {
            // Fail-silent logging to prevent app crashes
        }
    }
}
