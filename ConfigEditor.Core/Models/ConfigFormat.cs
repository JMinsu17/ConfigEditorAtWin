namespace ConfigEditor.Core.Models;

/// <summary>
/// Specifies the supported configuration file formats.
/// </summary>
public enum ConfigFormat
{
    /// <summary>
    /// Unknown or unsupported format.
    /// </summary>
    Unknown,

    /// <summary>
    /// JSON configuration format.
    /// </summary>
    Json,

    /// <summary>
    /// INI configuration format.
    /// </summary>
    Ini
}
