using ConfigEditor.Core.Models;

namespace ConfigEditor.App.ViewModels;

/// <summary>
/// ViewModel wrapping ValidationResult model for list rendering.
/// </summary>
public class ValidationResultViewModel
{
    public ValidationResult Model { get; }

    public bool IsValid => Model.IsValid;
    public string Path => Model.Path;
    public string Message => Model.Message;
    public ValidationSeverity Severity => Model.Severity;

    public ValidationResultViewModel(ValidationResult model)
    {
        Model = model;
    }
}
