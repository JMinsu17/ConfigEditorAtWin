using System;
using System.IO;
using System.Linq;
using Xunit;
using ConfigEditor.Core.Models;
using ConfigEditor.Formats.Ini;

namespace ConfigEditor.Tests;

/// <summary>
/// Tests for IniConfigParser and IniConfigWriter.
/// </summary>
public class IniConfigParserTests : IDisposable
{
    private readonly string _tempFilePath;

    public IniConfigParserTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ini");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    [Fact]
    public void Load_SimpleIni_ParsesCorrectly()
    {
        string ini = @"; Server Configurations
[server]
ip=192.168.10.120
port = 3000
enable_ssh = true

[database]
db_name=main_db";
        File.WriteAllText(_tempFilePath, ini);
        var parser = new IniConfigParser();

        var doc = parser.Load(_tempFilePath);

        Assert.Equal(ConfigFormat.Ini, doc.Format);
        Assert.Equal(2, doc.Nodes.Count);

        var serverSection = doc.Nodes.FirstOrDefault(n => n.Key == "server");
        Assert.NotNull(serverSection);
        Assert.Equal(ConfigValueType.Object, serverSection.ValueType);
        Assert.Equal(3, serverSection.Children.Count);

        var ipNode = serverSection.Children.FirstOrDefault(n => n.Key == "ip");
        Assert.NotNull(ipNode);
        Assert.Equal("192.168.10.120", ipNode.Value);
        Assert.Equal(ConfigValueType.String, ipNode.ValueType);

        var portNode = serverSection.Children.FirstOrDefault(n => n.Key == "port");
        Assert.NotNull(portNode);
        Assert.Equal("3000", portNode.Value);
        Assert.Equal(ConfigValueType.Integer, portNode.ValueType);

        var dbSection = doc.Nodes.FirstOrDefault(n => n.Key == "database");
        Assert.NotNull(dbSection);
        Assert.Single(dbSection.Children);
        Assert.Equal("db_name", dbSection.Children[0].Key);
        Assert.Equal("main_db", dbSection.Children[0].Value);
    }

    [Fact]
    public void Write_ModifiedValues_PreservesCommentsAndSpacing()
    {
        string ini = @"; Pre-comment
[server]
ip = 192.168.10.120
; Inline comment
port=3000
";
        File.WriteAllText(_tempFilePath, ini);
        var parser = new IniConfigParser();
        var doc = parser.Load(_tempFilePath);

        var server = doc.Nodes.First(n => n.Key == "server");
        server.Children.First(c => c.Key == "ip").Value = "10.0.0.1";
        server.Children.First(c => c.Key == "port").Value = "8080";

        var writer = new IniConfigWriter();

        string outputText = writer.BuildText(doc);

        Assert.Contains("; Pre-comment", outputText);
        Assert.Contains("; Inline comment", outputText);
        Assert.Contains("ip = 10.0.0.1", outputText);
        Assert.Contains("port=8080", outputText);
    }
}
