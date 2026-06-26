namespace ConfigEditor.Core.Models;

/// <summary>
/// Specifies the type of configuration values.
/// </summary>
public enum ConfigValueType
{
    String,
    Integer,
    Float,
    Boolean,
    Object,
    Array,
    Null,
    Unknown
}
