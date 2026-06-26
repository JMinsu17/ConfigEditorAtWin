using System;
using System.Collections.Generic;
using System.Text.Json;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Core.Models;

namespace ConfigEditor.Formats.Json;

/// <summary>
/// Validator for JSON configuration files and nodes.
/// </summary>
public class JsonConfigValidator : IConfigValidator
{
    public IReadOnlyList<ValidationResult> Validate(ConfigDocument document)
    {
        var results = new List<ValidationResult>();

        if (!string.IsNullOrWhiteSpace(document.CurrentText))
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(document.CurrentText);
            }
            catch (JsonException ex)
            {
                results.Add(new ValidationResult
                {
                    IsValid = false,
                    Path = $"Line: {ex.LineNumber ?? 0}, Position: {ex.BytePositionInLine ?? 0}",
                    Message = $"JSON Syntax Error: {ex.Message}",
                    Severity = ValidationSeverity.Error
                });
            }
        }
        else
        {
            results.Add(new ValidationResult
            {
                IsValid = false,
                Path = "",
                Message = "File content is empty.",
                Severity = ValidationSeverity.Warning
            });
        }

        ValidateNodes(document.Nodes, results);

        return results;
    }

    private void ValidateNodes(IEnumerable<ConfigNode> nodes, List<ValidationResult> results)
    {
        foreach (var node in nodes)
        {
            if (node.ValueType != ConfigValueType.Object && node.ValueType != ConfigValueType.Array)
            {
                switch (node.ValueType)
                {
                    case ConfigValueType.Boolean:
                        if (node.Value != null && !bool.TryParse(node.Value, out _))
                        {
                            results.Add(new ValidationResult
                            {
                                IsValid = false,
                                Path = node.Path,
                                Message = $"Value '{node.Value}' is not a valid boolean for key '{node.Key}'.",
                                Severity = ValidationSeverity.Error
                            });
                        }
                        break;

                    case ConfigValueType.Integer:
                        if (node.Value != null && !long.TryParse(node.Value, out _))
                        {
                            results.Add(new ValidationResult
                            {
                                IsValid = false,
                                Path = node.Path,
                                Message = $"Value '{node.Value}' is not a valid integer for key '{node.Key}'.",
                                Severity = ValidationSeverity.Error
                            });
                        }
                        break;

                    case ConfigValueType.Float:
                        if (node.Value != null && !double.TryParse(node.Value, out _))
                        {
                            results.Add(new ValidationResult
                            {
                                IsValid = false,
                                Path = node.Path,
                                Message = $"Value '{node.Value}' is not a valid float for key '{node.Key}'.",
                                Severity = ValidationSeverity.Error
                            });
                        }
                        break;
                }
            }

            if (node.Children.Count > 0)
            {
                ValidateNodes(node.Children, results);
            }
        }
    }
}
