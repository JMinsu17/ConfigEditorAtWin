namespace ConfigEditor.Core.Models;

/// <summary>
/// Represents the result of a validation check on a configuration element.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; }
}
