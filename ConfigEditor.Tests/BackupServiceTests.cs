using System;
using System.IO;
using Xunit;
using ConfigEditor.Infrastructure;

namespace ConfigEditor.Tests;

/// <summary>
/// Tests for BackupService.
/// </summary>
public class BackupServiceTests : IDisposable
{
    private readonly string _tempFilePath;

    public BackupServiceTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), "test_config.ini");
        File.WriteAllText(_tempFilePath, "port=3000");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }

        string backupDir = Path.Combine(Path.GetTempPath(), ".backup");
        if (Directory.Exists(backupDir))
        {
            try { Directory.Delete(backupDir, true); } catch { }
        }
    }

    [Fact]
    public void CreateBackup_Succeeds_WhenOriginalFileExists()
    {
        var backupService = new BackupService();

        var result = backupService.CreateBackup(_tempFilePath);

        Assert.True(result.IsSuccess);
        string backupPath = result.Value;
        Assert.True(File.Exists(backupPath));
        
        string folderName = Path.GetFileName(Path.GetDirectoryName(backupPath));
        Assert.Equal(".backup", folderName);

        string backupFileName = Path.GetFileName(backupPath);
        Assert.StartsWith("test_config_", backupFileName);
        Assert.EndsWith(".ini", backupFileName);
    }
}
