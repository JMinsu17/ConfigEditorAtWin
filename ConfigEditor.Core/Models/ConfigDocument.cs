using System.Collections.Generic;

namespace ConfigEditor.Core.Models;

/// <summary>
/// Represents a configuration document, holding settings and metadata.
/// </summary>
public class ConfigDocument
{
    public string FilePath { get; set; } = string.Empty;
    public ConfigFormat Format { get; set; }
    public List<ConfigNode> Nodes { get; set; } = new();
    public string OriginalText { get; set; } = string.Empty;
    public string CurrentText { get; set; } = string.Empty;
    public bool IsDirty { get; set; }

    /// <summary>
    /// Additional parser-specific metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
