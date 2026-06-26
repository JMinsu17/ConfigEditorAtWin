using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Core.Models;

namespace ConfigEditor.Formats.Json;

/// <summary>
/// Parser implementation for JSON configuration files.
/// </summary>
public class JsonConfigParser : IConfigParser
{
    public bool CanRead(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".json", System.StringComparison.OrdinalIgnoreCase);
    }

    public ConfigDocument Load(string filePath)
    {
        string text = File.ReadAllText(filePath);
        var doc = new ConfigDocument
        {
            FilePath = filePath,
            Format = ConfigFormat.Json,
            OriginalText = text,
            CurrentText = text,
            IsDirty = false
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            return doc;
        }

        var jsonOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        using var jsonDoc = JsonDocument.Parse(text, jsonOptions);
        var rootElement = jsonDoc.RootElement;
        
        doc.Nodes = ParseElement("", "", rootElement);
        return doc;
    }

    private List<ConfigNode> ParseElement(string key, string parentPath, JsonElement element)
    {
        var nodes = new List<ConfigNode>();
        string currentPath = string.IsNullOrEmpty(parentPath) 
            ? key 
            : (string.IsNullOrEmpty(key) ? parentPath : $"{parentPath}.{key}");

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var objNode = new ConfigNode
                {
                    Key = key,
                    DisplayName = string.IsNullOrEmpty(key) ? "root" : key,
                    Path = currentPath,
                    ValueType = ConfigValueType.Object,
                    Value = null,
                    OriginalValue = null
                };
                foreach (var prop in element.EnumerateObject())
                {
                    objNode.Children.AddRange(ParseElement(prop.Name, currentPath, prop.Value));
                }
                if (string.IsNullOrEmpty(key))
                {
                    nodes.AddRange(objNode.Children);
                }
                else
                {
                    nodes.Add(objNode);
                }
                break;

            case JsonValueKind.Array:
                var arrNode = new ConfigNode
                {
                    Key = key,
                    DisplayName = string.IsNullOrEmpty(key) ? "root" : key,
                    Path = currentPath,
                    ValueType = ConfigValueType.Array,
                    Value = null,
                    OriginalValue = null
                };
                int idx = 0;
                foreach (var item in element.EnumerateArray())
                {
                    arrNode.Children.AddRange(ParseElement(idx.ToString(), currentPath, item));
                    idx++;
                }
                if (string.IsNullOrEmpty(key))
                {
                    nodes.AddRange(arrNode.Children);
                }
                else
                {
                    nodes.Add(arrNode);
                }
                break;

            default:
                var valNode = new ConfigNode
                {
                    Key = key,
                    DisplayName = key,
                    Path = currentPath,
                    ValueType = MapValueType(element.ValueKind, element),
                    Value = element.ValueKind == JsonValueKind.Null ? null : GetElementRawString(element),
                    OriginalValue = element.ValueKind == JsonValueKind.Null ? null : GetElementRawString(element)
                };
                nodes.Add(valNode);
                break;
        }

        return nodes;
    }

    private string GetElementRawString(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
            return element.GetString() ?? "";
        return element.GetRawText();
    }

    private ConfigValueType MapValueType(JsonValueKind kind, JsonElement element)
    {
        return kind switch
        {
            JsonValueKind.String => ConfigValueType.String,
            JsonValueKind.True or JsonValueKind.False => ConfigValueType.Boolean,
            JsonValueKind.Number => element.TryGetInt64(out _) ? ConfigValueType.Integer : ConfigValueType.Float,
            JsonValueKind.Null => ConfigValueType.Null,
            JsonValueKind.Object => ConfigValueType.Object,
            JsonValueKind.Array => ConfigValueType.Array,
            _ => ConfigValueType.Unknown
        };
    }
}
