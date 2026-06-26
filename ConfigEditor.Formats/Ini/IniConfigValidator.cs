using System;
using System.Collections.Generic;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Core.Models;

namespace ConfigEditor.Formats.Ini;

/// <summary>
/// Validator implementation for INI configuration files.
/// </summary>
public class IniConfigValidator : IConfigValidator
{
    public IReadOnlyList<ValidationResult> Validate(ConfigDocument document)
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(document.CurrentText))
        {
            results.Add(new ValidationResult
            {
                IsValid = false,
                Path = "",
                Message = "File content is empty.",
                Severity = ValidationSeverity.Warning
            });
            return results;
        }

        string[] rawLines = document.CurrentText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        var sectionKeys = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var sectionHasKeys = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        string currentSection = "";

        for (int i = 0; i < rawLines.Length; i++)
        {
            int lineNum = i + 1;
            string rawLine = rawLines[i];
            string trimmed = rawLine.Trim();

            if (trimmed.Length == 0 || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
            {
                continue;
            }

            if (trimmed.StartsWith('[') || trimmed.EndsWith(']'))
            {
                if (!trimmed.StartsWith('[') || !trimmed.EndsWith(']'))
                {
                    results.Add(new ValidationResult
                    {
                        IsValid = false,
                        Path = $"Line {lineNum}",
                        Message = "Malformed section header. Missing '[' or ']'.",
                        Severity = ValidationSeverity.Error
                    });
                }
                else
                {
                    string sectionName = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    if (string.IsNullOrEmpty(sectionName))
                    {
                        results.Add(new ValidationResult
                        {
                            IsValid = false,
                            Path = $"Line {lineNum}",
                            Message = "Section name cannot be empty.",
                            Severity = ValidationSeverity.Error
                        });
                    }
                    else
                    {
                        currentSection = sectionName;
                        if (!sectionKeys.ContainsKey(currentSection))
                        {
                            sectionKeys[currentSection] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            sectionHasKeys[currentSection] = false;
                        }
                    }
                }
            }
            else if (trimmed.Contains('='))
            {
                int eqIndex = trimmed.IndexOf('=');
                string key = trimmed.Substring(0, eqIndex).Trim();
                if (string.IsNullOrEmpty(key))
                {
                    results.Add(new ValidationResult
                    {
                        IsValid = false,
                        Path = $"Line {lineNum}",
                        Message = "Key name cannot be empty.",
                        Severity = ValidationSeverity.Error
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentSection))
                    {
                        sectionHasKeys[currentSection] = true;
                    }

                    if (!sectionKeys.TryGetValue(currentSection, out var keys))
                    {
                        keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        sectionKeys[currentSection] = keys;
                    }

                    if (!keys.Add(key))
                    {
                        results.Add(new ValidationResult
                        {
                            IsValid = true,
                            Path = $"Line {lineNum} ({currentSection}.{key})",
                            Message = $"Duplicate key '{key}' found in section '{currentSection}'.",
                            Severity = ValidationSeverity.Warning
                        });
                    }
                }
            }
            else
            {
                results.Add(new ValidationResult
                {
                    IsValid = false,
                    Path = $"Line {lineNum}",
                    Message = "Line does not contain a '=' separator and is not a comment or section.",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        foreach (var secKvp in sectionHasKeys)
        {
            if (!secKvp.Value)
            {
                results.Add(new ValidationResult
                {
                    IsValid = true,
                    Path = $"Section: [{secKvp.Key}]",
                    Message = $"Section '{secKvp.Key}' is empty.",
                    Severity = ValidationSeverity.Warning
                });
            }
        }

        return results;
    }
}
