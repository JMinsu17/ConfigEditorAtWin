using System.Linq;
using Xunit;
using ConfigEditor.Core.Models;
using ConfigEditor.Formats.Ini;
using ConfigEditor.Formats.Json;

namespace ConfigEditor.Tests;

/// <summary>
/// Tests for JSON and INI validation logic.
/// </summary>
public class ValidationTests
{
    [Fact]
    public void Validate_InvalidJson_ReturnsSyntaxError()
    {
        var doc = new ConfigDocument
        {
            Format = ConfigFormat.Json,
            FilePath = "test.json",
            CurrentText = @"{ ""port"": 3000 "
        };
        var validator = new JsonConfigValidator();

        var results = validator.Validate(doc);

        Assert.NotEmpty(results);
        var syntaxError = results.FirstOrDefault(r => r.Severity == ValidationSeverity.Error);
        Assert.NotNull(syntaxError);
        Assert.Contains("JSON Syntax Error", syntaxError.Message);
    }

    [Fact]
    public void Validate_InvalidIni_ReturnsMalformedSectionAndDuplicateKey()
    {
        var doc = new ConfigDocument
        {
            Format = ConfigFormat.Ini,
            FilePath = "test.ini",
            CurrentText = @"[server
port=3000
port=4000
invalid_line_no_equals"
        };
        var validator = new IniConfigValidator();

        var results = validator.Validate(doc);

        Assert.NotEmpty(results);

        var sectionError = results.FirstOrDefault(r => r.Message.Contains("Malformed section"));
        Assert.NotNull(sectionError);
        Assert.Equal(ValidationSeverity.Error, sectionError.Severity);

        var duplicateWarning = results.FirstOrDefault(r => r.Message.Contains("Duplicate key"));
        Assert.NotNull(duplicateWarning);
        Assert.Equal(ValidationSeverity.Warning, duplicateWarning.Severity);

        var syntaxError = results.FirstOrDefault(r => r.Message.Contains("does not contain a '='"));
        Assert.NotNull(syntaxError);
        Assert.Equal(ValidationSeverity.Error, syntaxError.Severity);
    }
}
