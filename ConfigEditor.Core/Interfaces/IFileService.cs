using System.Text;
using ConfigEditor.Core.Common;

namespace ConfigEditor.Core.Interfaces;

/// <summary>
/// Defines file operations with encoding detection and validation support.
/// </summary>
public interface IFileService
{
    Result<string> ReadAllText(string filePath);
    Result WriteAllText(string filePath, string content, Encoding? encoding = null);
    bool Exists(string filePath);
    bool IsReadOnly(string filePath);
    Result CheckWritePermission(string filePath);
}
