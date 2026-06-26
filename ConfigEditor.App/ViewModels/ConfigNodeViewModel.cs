using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigEditor.Core.Models;

namespace ConfigEditor.App.ViewModels;

/// <summary>
/// ViewModel representing a single configuration node in the settings tree.
/// </summary>
public partial class ConfigNodeViewModel : ObservableObject
{
    private readonly Action<ConfigNodeViewModel> _onValueChanged;
    private string? _value;
    private bool _isExpanded;
    private bool _isSelected;
    private bool _isModified;

    public ConfigNode Model { get; }

    public string Key => Model.Key;
    public string DisplayName => Model.DisplayName;
    public string Path => Model.Path;
    public string? Section => Model.Section;
    public ConfigValueType ValueType => Model.ValueType;
    public ObservableCollection<ConfigNodeViewModel> Children { get; }
    public string? Description => Model.Description;
    public string? OriginalValue => Model.OriginalValue;

    public string? Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                Model.Value = value;
                IsModified = true;
                _onValueChanged(this);
            }
        }
    }

    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (SetProperty(ref _isModified, value))
            {
                Model.IsModified = value;
            }
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public ConfigNodeViewModel(ConfigNode model, Action<ConfigNodeViewModel> onValueChanged)
    {
        Model = model;
        _onValueChanged = onValueChanged;
        _value = model.Value;
        _isModified = model.IsModified;
        
        Children = new ObservableCollection<ConfigNodeViewModel>(
            model.Children.Select(c => new ConfigNodeViewModel(c, onValueChanged))
        );
    }

    public void ResetModified()
    {
        IsModified = false;
        Model.OriginalValue = Model.Value;
        OnPropertyChanged(nameof(OriginalValue));
        foreach (var child in Children)
        {
            child.ResetModified();
        }
    }

    [RelayCommand]
    public void Revert()
    {
        if (IsModified)
        {
            _value = Model.OriginalValue;
            Model.Value = Model.OriginalValue;
            OnPropertyChanged(nameof(Value));
            IsModified = false;
            _onValueChanged(this);
        }
    }
}
