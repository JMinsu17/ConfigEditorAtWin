using System;
using System.Collections.Generic;
using System.IO;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Core.Models;

namespace ConfigEditor.Formats.Ini;

/// <summary>
/// Parser implementation for INI and CFG configuration files.
/// </summary>
public class IniConfigParser : IConfigParser
{
    public bool CanRead(string filePath)
    {
        string ext = Path.GetExtension(filePath);
        return ext.Equals(".ini", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".cfg", StringComparison.OrdinalIgnoreCase);
    }

    public ConfigDocument Load(string filePath)
    {
        string text = File.ReadAllText(filePath);
        var doc = new ConfigDocument
        {
            FilePath = filePath,
            Format = ConfigFormat.Ini,
            OriginalText = text,
            CurrentText = text,
            IsDirty = false
        };

        var lines = new List<IniLine>();
        var sections = new Dictionary<string, ConfigNode>();
        var rootNodes = new List<ConfigNode>();
        
        string[] rawLines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        string currentSection = "";
        ConfigNode? currentSectionNode = null;

        for (int i = 0; i < rawLines.Length; i++)
        {
            string rawLine = rawLines[i];
            string trimmed = rawLine.Trim();

            if (trimmed.Length == 0)
            {
                lines.Add(new IniEmptyLine());
            }
            else if (trimmed.StartsWith(';') || trimmed.StartsWith('#'))
            {
                lines.Add(new IniCommentLine(rawLine));
            }
            else if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                string sectionName = trimmed.Substring(1, trimmed.Length - 2).Trim();
                currentSection = sectionName;
                lines.Add(new IniSectionLine(sectionName, rawLine));

                if (!sections.TryGetValue(sectionName, out currentSectionNode))
                {
                    currentSectionNode = new ConfigNode
                    {
                        Key = sectionName,
                        DisplayName = sectionName,
                        Path = sectionName,
                        ValueType = ConfigValueType.Object,
                        Section = sectionName
                    };
                    sections[sectionName] = currentSectionNode;
                    rootNodes.Add(currentSectionNode);
                }
            }
            else if (trimmed.Contains('='))
            {
                int eqIndex = rawLine.IndexOf('=');
                string key = rawLine.Substring(0, eqIndex).Trim();
                string val = rawLine.Substring(eqIndex + 1).Trim();

                var kvLine = new IniKeyValueLine(currentSection, key, val, rawLine);
                lines.Add(kvLine);

                var node = new ConfigNode
                {
                    Key = key,
                    DisplayName = key,
                    Value = val,
                    OriginalValue = val,
                    Path = string.IsNullOrEmpty(currentSection) ? key : $"{currentSection}.{key}",
                    Section = currentSection,
                    ValueType = InferIniValueType(val)
                };

                if (string.IsNullOrEmpty(currentSection))
                {
                    rootNodes.Add(node);
                }
                else
                {
                    currentSectionNode!.Children.Add(node);
                }
            }
            else
            {
                lines.Add(new IniUnknownLine(rawLine));
            }
        }

        doc.Nodes = rootNodes;
        doc.Metadata["IniLines"] = lines;
        return doc;
    }

    private ConfigValueType InferIniValueType(string value)
    {
        if (string.IsNullOrEmpty(value))
            return ConfigValueType.String;

        if (bool.TryParse(value, out _) ||
            value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("on", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            return ConfigValueType.Boolean;
        }

        if (long.TryParse(value, out _))
            return ConfigValueType.Integer;

        if (double.TryParse(value, out _))
            return ConfigValueType.Float;

        return ConfigValueType.String;
    }
}
