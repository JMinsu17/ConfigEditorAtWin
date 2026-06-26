using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigEditor.Core.Models;
using ConfigEditor.Formats;

namespace ConfigEditor.App.ViewModels;

/// <summary>
/// ViewModel wrapping ConfigDocument to track current content, nodes, and dirty state.
/// </summary>
public class ConfigDocumentViewModel : ObservableObject
{
    private readonly ConfigParserFactory _parserFactory;
    private string _currentText = string.Empty;
    private bool _isDirty;
    private string _filePath = string.Empty;

    public ConfigDocument Model { get; }

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (SetProperty(ref _filePath, value))
            {
                Model.FilePath = value;
                OnPropertyChanged(nameof(FileName));
            }
        }
    }

    public string FileName => string.IsNullOrEmpty(FilePath) ? "Untitled" : Path.GetFileName(FilePath);
    public ConfigFormat Format => Model.Format;

    public ObservableCollection<ConfigNodeViewModel> Nodes { get; } = new();

    public string CurrentText
    {
        get => _currentText;
        set
        {
            if (SetProperty(ref _currentText, value))
            {
                Model.CurrentText = value;
                IsDirty = true;
            }
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (SetProperty(ref _isDirty, value))
            {
                Model.IsDirty = value;
            }
        }
    }

    public ConfigDocumentViewModel(ConfigDocument model, ConfigParserFactory parserFactory)
    {
        Model = model;
        _parserFactory = parserFactory;
        _filePath = model.FilePath;
        _currentText = model.CurrentText;
        _isDirty = model.IsDirty;

        LoadNodes();
    }

    public void LoadNodes()
    {
        Nodes.Clear();
        foreach (var node in Model.Nodes)
        {
            Nodes.Add(new ConfigNodeViewModel(node, OnNodeValueChanged));
        }
    }

    private void OnNodeValueChanged(ConfigNodeViewModel node)
    {
        IsDirty = true;
    }

    public void SyncFromTreeToText()
    {
        try
        {
            var writer = _parserFactory.GetWriter(FilePath);
            string newText = writer.BuildText(Model);
            _currentText = newText;
            Model.CurrentText = newText;
            OnPropertyChanged(nameof(CurrentText));
        }
        catch
        {
            // Ignore syntax/formatting errors during dynamic synchronization
        }
    }

    public bool SyncFromTextToTree()
    {
        try
        {
            var parser = _parserFactory.GetParser(FilePath);
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(FilePath));
            File.WriteAllText(tempFile, CurrentText);
            
            var tempDoc = parser.Load(tempFile);
            try { File.Delete(tempFile); } catch { }

            Model.Nodes = tempDoc.Nodes;
            if (tempDoc.Metadata.TryGetValue("IniLines", out var iniLines))
            {
                Model.Metadata["IniLines"] = iniLines;
            }
            
            LoadNodes();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ResetDirty()
    {
        IsDirty = false;
        Model.OriginalText = CurrentText;
        foreach (var node in Nodes)
        {
            node.ResetModified();
        }
    }
}
