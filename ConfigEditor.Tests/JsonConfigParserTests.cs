using System;
using System.IO;
using System.Linq;
using Xunit;
using ConfigEditor.Core.Models;
using ConfigEditor.Formats.Json;

namespace ConfigEditor.Tests;

/// <summary>
/// Tests for JsonConfigParser and JsonConfigWriter.
/// </summary>
public class JsonConfigParserTests : IDisposable
{
    private readonly string _tempFilePath;

    public JsonConfigParserTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    [Fact]
    public void Load_SimpleObject_ParsesCorrectly()
    {
        string json = @"{
            ""server_ip"": ""192.168.10.120"",
            ""port"": 3000,
            ""enable_ssh"": true
        }";
        File.WriteAllText(_tempFilePath, json);
        var parser = new JsonConfigParser();

        var doc = parser.Load(_tempFilePath);

        Assert.Equal(ConfigFormat.Json, doc.Format);
        Assert.Equal(3, doc.Nodes.Count);

        var ipNode = doc.Nodes.FirstOrDefault(n => n.Key == "server_ip");
        Assert.NotNull(ipNode);
        Assert.Equal("192.168.10.120", ipNode.Value);
        Assert.Equal(ConfigValueType.String, ipNode.ValueType);

        var portNode = doc.Nodes.FirstOrDefault(n => n.Key == "port");
        Assert.NotNull(portNode);
        Assert.Equal("3000", portNode.Value);
        Assert.Equal(ConfigValueType.Integer, portNode.ValueType);

        var sshNode = doc.Nodes.FirstOrDefault(n => n.Key == "enable_ssh");
        Assert.NotNull(sshNode);
        Assert.Equal("True", sshNode.Value, ignoreCase: true);
        Assert.Equal(ConfigValueType.Boolean, sshNode.ValueType);
    }

    [Fact]
    public void Load_NestedObject_ParsesHierarchy()
    {
        string json = @"{
            ""server"": {
                ""ip"": ""192.168.10.120"",
                ""port"": 3000
            }
        }";
        File.WriteAllText(_tempFilePath, json);
        var parser = new JsonConfigParser();

        var doc = parser.Load(_tempFilePath);

        Assert.Single(doc.Nodes);
        var serverNode = doc.Nodes[0];
        Assert.Equal("server", serverNode.Key);
        Assert.Equal(ConfigValueType.Object, serverNode.ValueType);
        Assert.Equal(2, serverNode.Children.Count);

        var ipNode = serverNode.Children.FirstOrDefault(n => n.Key == "ip");
        Assert.NotNull(ipNode);
        Assert.Equal("server.ip", ipNode.Path);
        Assert.Equal("192.168.10.120", ipNode.Value);

        var portNode = serverNode.Children.FirstOrDefault(n => n.Key == "port");
        Assert.NotNull(portNode);
        Assert.Equal("server.port", portNode.Path);
        Assert.Equal("3000", portNode.Value);
    }

    [Fact]
    public void Write_ModifiedValues_WritesCorrectJson()
    {
        string json = @"{
            ""ip"": ""127.0.0.1"",
            ""port"": 80
        }";
        File.WriteAllText(_tempFilePath, json);
        var parser = new JsonConfigParser();
        var doc = parser.Load(_tempFilePath);

        var ipNode = doc.Nodes.First(n => n.Key == "ip");
        ipNode.Value = "10.0.0.1";
        
        var portNode = doc.Nodes.First(n => n.Key == "port");
        portNode.Value = "8080";

        var writer = new JsonConfigWriter();

        string outputText = writer.BuildText(doc);

        Assert.Contains(@"""ip"": ""10.0.0.1""", outputText);
        Assert.Contains(@"""port"": 8080", outputText);
    }
}
