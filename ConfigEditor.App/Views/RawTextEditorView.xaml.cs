using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using ConfigEditor.App.ViewModels;

namespace ConfigEditor.App.Views;

/// <summary>
/// Interaction logic for RawTextEditorView.xaml
/// </summary>
public partial class RawTextEditorView : UserControl
{
    private bool _isUpdatingText;

    public RawTextEditorView()
    {
        InitializeComponent();
        DataContextChanged += RawTextEditorView_DataContextChanged;
    }

    private void RawTextEditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
        {
            oldVm.PropertyChanged -= MainVm_PropertyChanged;
            if (oldVm.SelectedDocument != null)
            {
                oldVm.SelectedDocument.PropertyChanged -= SelectedDoc_PropertyChanged;
            }
        }
        if (e.NewValue is MainViewModel newVm)
        {
            newVm.PropertyChanged += MainVm_PropertyChanged;
            if (newVm.SelectedDocument != null)
            {
                newVm.SelectedDocument.PropertyChanged += SelectedDoc_PropertyChanged;
            }
            UpdateEditor(newVm.SelectedDocument);
        }
    }

    private void MainVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedDocument))
        {
            if (DataContext is MainViewModel vm)
            {
                UpdateEditor(vm.SelectedDocument);
            }
        }
    }

    private void SelectedDoc_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConfigDocumentViewModel.CurrentText))
        {
            if (DataContext is MainViewModel vm)
            {
                _isUpdatingText = true;
                TextEditor.Text = vm.SelectedDocument?.CurrentText ?? string.Empty;
                _isUpdatingText = false;
            }
        }
    }

    private void UpdateEditor(ConfigDocumentViewModel? doc)
    {
        if (doc == null)
        {
            TextEditor.Text = string.Empty;
            TextEditor.IsEnabled = false;
            return;
        }

        TextEditor.IsEnabled = true;
        
        _isUpdatingText = true;
        TextEditor.Text = doc.CurrentText;
        _isUpdatingText = false;

        string ext = Path.GetExtension(doc.FilePath).ToLower();
        if (ext == ".json")
        {
            TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");
        }
        else
        {
            TextEditor.SyntaxHighlighting = null;
        }
    }

    private void TextEditor_TextChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingText)
            return;

        if (DataContext is MainViewModel vm && vm.SelectedDocument != null)
        {
            vm.SelectedDocument.CurrentText = TextEditor.Text;
        }
    }
}
