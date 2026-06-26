using ConfigEditor.Core.Common;

namespace ConfigEditor.Core.Interfaces;

/// <summary>
/// Defines methods to create and manage file backups.
/// </summary>
public interface IBackupService
{
    Result<string> CreateBackup(string filePath);
}
