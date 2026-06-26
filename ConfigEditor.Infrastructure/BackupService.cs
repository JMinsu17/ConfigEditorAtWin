using System;
using System.IO;
using ConfigEditor.Core.Common;
using ConfigEditor.Core.Interfaces;

namespace ConfigEditor.Infrastructure;

/// <summary>
/// Service to perform automatic backups in a .backup subdirectory.
/// </summary>
public class BackupService : IBackupService
{
    public Result<string> CreateBackup(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return Result<string>.Failure("ORIGINAL_FILE_NOT_FOUND", "Original file does not exist. Backup skipped.");
            }

            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }

            string backupDir = Path.Combine(directory, ".backup");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"{fileName}_{timestamp}{extension}";
            string backupPath = Path.Combine(backupDir, backupFileName);

            File.Copy(filePath, backupPath, true);

            return Result<string>.Success(backupPath);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure("BACKUP_FAILED", $"Failed to create backup: {ex.Message}");
        }
    }
}
