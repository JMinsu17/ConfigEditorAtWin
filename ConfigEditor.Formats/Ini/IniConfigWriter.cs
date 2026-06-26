using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Core.Models;

namespace ConfigEditor.Formats.Ini;

/// <summary>
/// Writer implementation for INI configuration files.
/// </summary>
public class IniConfigWriter : IConfigWriter
{
    public void Save(ConfigDocument document, string filePath)
    {
        string text = BuildText(document);
        File.WriteAllText(filePath, text);
        document.CurrentText = text;
        document.IsDirty = false;
    }

    public string BuildText(ConfigDocument document)
    {
        var nodesMap = new Dictionary<(string Section, string Key), ConfigNode>();
        var sectionsInNodes = new HashSet<string>();

        void BuildMap(IEnumerable<ConfigNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.ValueType == ConfigValueType.Object)
                {
                    sectionsInNodes.Add(node.Key);
                    BuildMap(node.Children);
                }
                else
                {
                    nodesMap[(node.Section ?? "", node.Key)] = node;
                }
            }
        }
        BuildMap(document.Nodes);

        List<IniLine> originalLines;
        if (document.Metadata.TryGetValue("IniLines", out var linesObj) && linesObj is List<IniLine> list)
        {
            originalLines = list;
        }
        else
        {
            originalLines = new List<IniLine>();
            foreach (var node in document.Nodes)
            {
                if (node.ValueType == ConfigValueType.Object)
                {
                    originalLines.Add(new IniSectionLine(node.Key, $"[{node.Key}]"));
                    foreach (var child in node.Children)
                    {
                        originalLines.Add(new IniKeyValueLine(node.Key, child.Key, child.Value ?? "", $"{child.Key}={child.Value}"));
                    }
                }
                else
                {
                    originalLines.Add(new IniKeyValueLine("", node.Key, node.Value ?? "", $"{node.Key}={node.Value}"));
                }
            }
        }

        var sb = new StringBuilder();
        var writtenKeys = new HashSet<(string Section, string Key)>();
        string currentSection = "";

        void AppendNewKeysForSection(string section)
        {
            foreach (var kvp in nodesMap)
            {
                if (kvp.Key.Section.Equals(section, StringComparison.OrdinalIgnoreCase))
                {
                    if (!writtenKeys.Contains(kvp.Key))
                    {
                        sb.AppendLine($"{kvp.Key.Key}={kvp.Value.Value}");
                        writtenKeys.Add(kvp.Key);
                    }
                }
            }
        }

        for (int i = 0; i < originalLines.Count; i++)
        {
            var line = originalLines[i];

            if (line is IniSectionLine sectionLine)
            {
                if (i > 0)
                {
                    AppendNewKeysForSection(currentSection);
                }

                currentSection = sectionLine.Name;
                sb.AppendLine(sectionLine.OriginalText);
            }
            else if (line is IniKeyValueLine kvLine)
            {
                var key = (kvLine.Section, kvLine.Key);
                if (nodesMap.TryGetValue(key, out var node))
                {
                    int eqIndex = kvLine.OriginalText.IndexOf('=');
                    if (eqIndex >= 0)
                    {
                        string leftPart = kvLine.OriginalText.Substring(0, eqIndex + 1);
                        string rightPart = kvLine.OriginalText.Substring(eqIndex + 1);
                        string spacesAfterEq = "";
                        foreach (char c in rightPart)
                        {
                            if (char.IsWhiteSpace(c))
                                spacesAfterEq += c;
                            else
                                break;
                        }
                        sb.AppendLine(leftPart + spacesAfterEq + (node.Value ?? ""));
                    }
                    else
                    {
                        sb.AppendLine($"{kvLine.Key}={node.Value}");
                    }
                    writtenKeys.Add(key);
                }
            }
            else if (line is IniCommentLine commentLine)
            {
                sb.AppendLine(commentLine.Text);
            }
            else if (line is IniEmptyLine)
            {
                sb.AppendLine();
            }
            else if (line is IniUnknownLine unknownLine)
            {
                sb.AppendLine(unknownLine.OriginalText);
            }
        }

        AppendNewKeysForSection(currentSection);

        foreach (var section in sectionsInNodes)
        {
            bool sectionHasUnwrittenKeys = false;
            foreach (var kvp in nodesMap)
            {
                if (kvp.Key.Section.Equals(section, StringComparison.OrdinalIgnoreCase) && !writtenKeys.Contains(kvp.Key))
                {
                    sectionHasUnwrittenKeys = true;
                    break;
                }
            }

            if (sectionHasUnwrittenKeys)
            {
                sb.AppendLine();
                sb.AppendLine($"[{section}]");
                AppendNewKeysForSection(section);
            }
        }

        return sb.ToString().TrimEnd() + Environment.NewLine;
    }
}
