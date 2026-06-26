namespace ConfigEditor.Formats.Ini;

/// <summary>
/// Represents a generic line in an INI file.
/// </summary>
public abstract record IniLine;

/// <summary>
/// Represents a section line, e.g. [server]
/// </summary>
public record IniSectionLine(string Name, string OriginalText) : IniLine;

/// <summary>
/// Represents a key-value line, e.g. ip=192.168.10.120
/// </summary>
public record IniKeyValueLine(
    string Section,
    string Key,
    string Value,
    string OriginalText
) : IniLine;

/// <summary>
/// Represents a comment line, e.g. ; server settings or # server settings
/// </summary>
public record IniCommentLine(string Text) : IniLine;

/// <summary>
/// Represents an empty or whitespace line.
/// </summary>
public record IniEmptyLine() : IniLine;

/// <summary>
/// Represents a line that cannot be parsed into sections, key-values, or comments.
/// </summary>
public record IniUnknownLine(string OriginalText) : IniLine;
