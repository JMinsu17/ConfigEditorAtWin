using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ConfigEditor.App.Services;

/// <summary>
/// Settings model saved in appsettings.json.
/// </summary>
public class AppSettings
{
    public List<string> RecentFiles { get; set; } = new();
    public string LastDirectory { get; set; } = string.Empty;
    public string Theme { get; set; } = "Light";
    public int JsonIndentSize { get; set; } = 4;
    public bool EnableBackup { get; set; } = true;
    public int BackupRetentionCount { get; set; } = 10;
}

/// <summary>
/// Service to load and save program settings from LocalAppData.
/// </summary>
public class AppSettingsService
{
    private readonly string _settingsFilePath;
    public AppSettings Settings { get; private set; } = new();

    public AppSettingsService()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appDir = Path.Combine(localAppData, "ConfigEditor");
        if (!Directory.Exists(appDir))
        {
            Directory.CreateDirectory(appDir);
        }
        _settingsFilePath = Path.Combine(appDir, "appsettings.json");
        LoadSettings();
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string text = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(text);
                if (settings != null)
                {
                    settings.RecentFiles ??= new List<string>();
                    settings.LastDirectory ??= string.Empty;
                    settings.Theme = string.IsNullOrWhiteSpace(settings.Theme) ? "Light" : settings.Theme;
                    Settings = settings;
                    return;
                }
            }
        }
        catch
        {
            // Ignore and fallback to defaults
        }
        Settings = new AppSettings();
    }

    public void SaveSettings()
    {
        try
        {
            string text = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, text);
        }
        catch
        {
            // Ignore setting save errors
        }
    }
}
