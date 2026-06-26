using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigEditor.App.Services;

/// <summary>
/// Service to manage the list of recently opened configuration files.
/// </summary>
public class RecentFileService
{
    private readonly AppSettingsService _settingsService;
    private const int MaxRecentFiles = 10;

    public RecentFileService(AppSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyList<string> GetRecentFiles()
    {
        return _settingsService.Settings.RecentFiles;
    }

    public void AddFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        var list = _settingsService.Settings.RecentFiles;
        list.RemoveAll(x => x.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        list.Insert(0, filePath);
        
        if (list.Count > MaxRecentFiles)
        {
            _settingsService.Settings.RecentFiles = list.Take(MaxRecentFiles).ToList();
        }
        _settingsService.SaveSettings();
    }

    public void RemoveFile(string filePath)
    {
        var list = _settingsService.Settings.RecentFiles;
        list.RemoveAll(x => x.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        _settingsService.SaveSettings();
    }
}
