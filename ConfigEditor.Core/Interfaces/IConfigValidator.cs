using System.Collections.Generic;
using ConfigEditor.Core.Models;

namespace ConfigEditor.Core.Interfaces;

/// <summary>
/// Defines methods to validate configuration documents.
/// </summary>
public interface IConfigValidator
{
    IReadOnlyList<ValidationResult> Validate(ConfigDocument document);
}
