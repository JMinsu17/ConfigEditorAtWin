using System.Collections.Generic;

namespace ConfigEditor.Core.Models;

/// <summary>
/// Represents a configuration node in the hierarchical settings tree.
/// </summary>
public class ConfigNode
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? OriginalValue { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Section { get; set; }
    public ConfigValueType ValueType { get; set; }
    public List<ConfigNode> Children { get; set; } = new();
    public bool IsModified { get; set; }
    public string? Description { get; set; }
}
