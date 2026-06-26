using System;
using System.IO;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Formats.Ini;
using ConfigEditor.Formats.Json;

namespace ConfigEditor.Formats;

/// <summary>
/// Factory to provide the appropriate parser, writer, and validator based on file extension.
/// </summary>
public class ConfigParserFactory
{
    private readonly JsonConfigParser _jsonParser = new();
    private readonly IniConfigParser _iniParser = new();

    private readonly JsonConfigWriter _jsonWriter = new();
    private readonly IniConfigWriter _iniWriter = new();

    private readonly JsonConfigValidator _jsonValidator = new();
    private readonly IniConfigValidator _iniValidator = new();

    public IConfigParser GetParser(string filePath)
    {
        if (_jsonParser.CanRead(filePath))
            return _jsonParser;
        if (_iniParser.CanRead(filePath))
            return _iniParser;

        throw new NotSupportedException($"Unsupported file format for: {filePath}");
    }

    public IConfigWriter GetWriter(string filePath)
    {
        string ext = Path.GetExtension(filePath);
        if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
            return _jsonWriter;
        if (ext.Equals(".ini", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".cfg", StringComparison.OrdinalIgnoreCase))
            return _iniWriter;

        throw new NotSupportedException($"Unsupported file format for: {filePath}");
    }

    public IConfigValidator GetValidator(string filePath)
    {
        string ext = Path.GetExtension(filePath);
        if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
            return _jsonValidator;
        if (ext.Equals(".ini", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".cfg", StringComparison.OrdinalIgnoreCase))
            return _iniValidator;

        throw new NotSupportedException($"Unsupported file format for: {filePath}");
    }
}
