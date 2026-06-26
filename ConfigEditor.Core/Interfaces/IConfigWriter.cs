using ConfigEditor.Core.Models;

namespace ConfigEditor.Core.Interfaces;

/// <summary>
/// Defines methods to write configuration files.
/// </summary>
public interface IConfigWriter
{
    void Save(ConfigDocument document, string filePath);
    string BuildText(ConfigDocument document);
}
