using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Core.Models;

namespace ConfigEditor.Formats.Json;

/// <summary>
/// Writer implementation for JSON configuration files.
/// </summary>
public class JsonConfigWriter : IConfigWriter
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
        if (document.Nodes.Count == 0)
            return "{}";

        bool isArray = document.OriginalText.TrimStart().StartsWith("[");

        JsonNode? rootNode;
        if (isArray)
        {
            var jsonArray = new JsonArray();
            foreach (var node in document.Nodes)
            {
                jsonArray.Add(BuildJsonNode(node));
            }
            rootNode = jsonArray;
        }
        else
        {
            var jsonObject = new JsonObject();
            foreach (var node in document.Nodes)
            {
                jsonObject.Add(node.Key, BuildJsonNode(node));
            }
            rootNode = jsonObject;
        }

        int indentSize = 4;
        if (document.Metadata.TryGetValue("IndentSize", out var sizeObj) && sizeObj is int size)
        {
            indentSize = size;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IndentCharacter = ' ',
            IndentSize = indentSize
        };

        return JsonSerializer.Serialize(rootNode, options);
    }

    private JsonNode? BuildJsonNode(ConfigNode node)
    {
        switch (node.ValueType)
        {
            case ConfigValueType.Object:
                var obj = new JsonObject();
                foreach (var child in node.Children)
                {
                    obj.Add(child.Key, BuildJsonNode(child));
                }
                return obj;

            case ConfigValueType.Array:
                var arr = new JsonArray();
                foreach (var child in node.Children)
                {
                    arr.Add(BuildJsonNode(child));
                }
                return arr;

            case ConfigValueType.Null:
                return null;

            case ConfigValueType.Boolean:
                if (bool.TryParse(node.Value, out bool bVal))
                    return JsonValue.Create(bVal);
                return JsonValue.Create(false);

            case ConfigValueType.Integer:
                if (long.TryParse(node.Value, out long iVal))
                    return JsonValue.Create(iVal);
                return JsonValue.Create(node.Value);

            case ConfigValueType.Float:
                if (double.TryParse(node.Value, out double fVal))
                    return JsonValue.Create(fVal);
                return JsonValue.Create(node.Value);

            case ConfigValueType.String:
            default:
                return JsonValue.Create(node.Value ?? "");
        }
    }
}
