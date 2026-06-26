using ConfigEditor.Core.Models;

namespace ConfigEditor.Core.Interfaces;

/// <summary>
/// Defines methods to parse configuration files.
/// </summary>
public interface IConfigParser
{
    bool CanRead(string filePath);
    ConfigDocument Load(string filePath);
}
