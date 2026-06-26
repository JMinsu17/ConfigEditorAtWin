using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ConfigEditor.App.ViewModels;
using ConfigEditor.Core.Models;

namespace ConfigEditor.App.Views;

/// <summary>
/// Interaction logic for ConfigFormEditorView.xaml
/// </summary>
public partial class ConfigFormEditorView : UserControl
{
    private ConfigNodeViewModel? _activeNode;

    public ConfigFormEditorView()
    {
        InitializeComponent();
        DataContextChanged += ConfigFormEditorView_DataContextChanged;
    }

    private void ConfigFormEditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
        {
            oldVm.PropertyChanged -= MainVm_PropertyChanged;
        }
        if (e.NewValue is MainViewModel newVm)
        {
            newVm.PropertyChanged += MainVm_PropertyChanged;
            UpdateUI(newVm.SelectedNode);
        }
    }

    private void MainVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedNode))
        {
            if (DataContext is MainViewModel vm)
            {
                UpdateUI(vm.SelectedNode);
            }
        }
    }

    private void UpdateUI(ConfigNodeViewModel? node)
    {
        if (_activeNode != null)
        {
            _activeNode.PropertyChanged -= Node_PropertyChanged;
        }
        _activeNode = node;

        if (node == null)
        {
            EmptyStateText.Visibility = Visibility.Visible;
            FormScrollViewer.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyStateText.Visibility = Visibility.Collapsed;
        FormScrollViewer.Visibility = Visibility.Visible;

        // Path (hide if identical to display name)
        if (string.IsNullOrEmpty(node.Path) || node.Path.Equals(node.DisplayName, System.StringComparison.OrdinalIgnoreCase))
        {
            NodePathText.Visibility = Visibility.Collapsed;
        }
        else
        {
            NodePathText.Visibility = Visibility.Visible;
            NodePathText.Text = node.Path;
        }

        NodeNameText.Text = node.DisplayName;

        // Description (hide if empty)
        if (string.IsNullOrWhiteSpace(node.Description))
        {
            NodeDescriptionText.Visibility = Visibility.Collapsed;
        }
        else
        {
            NodeDescriptionText.Visibility = Visibility.Visible;
            NodeDescriptionText.Text = node.Description;
        }

        // Type Badge styling
        string typeStr = node.ValueType.ToString().ToLower();
        NodeTemplateTypeText.Text = typeStr;
        NodeTemplateTypeBadge.Background = node.ValueType switch
        {
            ConfigValueType.String => GetThemeBrush("PrimaryBrush"),
            ConfigValueType.Integer => GetThemeBrush("AccentBrush"),
            ConfigValueType.Float => GetThemeBrush("AccentHoverBrush"),
            ConfigValueType.Boolean => GetThemeBrush("PrimaryHoverBrush"),
            ConfigValueType.Object => GetThemeBrush("PrimaryPressedBrush"),
            ConfigValueType.Array => GetThemeBrush("AccentHoverBrush"),
            _ => GetThemeBrush("TextMutedBrush")
        };

        // Modified and Revert Panel states
        ModifiedStarText.Visibility = node.IsModified ? Visibility.Visible : Visibility.Collapsed;
        RevertPanel.Visibility = node.IsModified ? Visibility.Visible : Visibility.Collapsed;
        OriginalValueText.Text = node.OriginalValue ?? "N/A";

        node.PropertyChanged += Node_PropertyChanged;

        if (node.ValueType == ConfigValueType.Object || node.ValueType == ConfigValueType.Array)
        {
            SectionEditorPanel.Visibility = Visibility.Visible;
            SingleEditorPanel.Visibility = Visibility.Collapsed;

            ChildrenItemsControl.ItemsSource = node.Children;
        }
        else
        {
            SectionEditorPanel.Visibility = Visibility.Collapsed;
            SingleEditorPanel.Visibility = Visibility.Visible;

            SingleValueContentControl.Content = node;
        }
    }

    private void Node_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is ConfigNodeViewModel node)
        {
            if (e.PropertyName == nameof(ConfigNodeViewModel.IsModified))
            {
                ModifiedStarText.Visibility = node.IsModified ? Visibility.Visible : Visibility.Collapsed;
                RevertPanel.Visibility = node.IsModified ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (e.PropertyName == nameof(ConfigNodeViewModel.OriginalValue))
            {
                OriginalValueText.Text = node.OriginalValue ?? "N/A";
            }
        }
    }

    private void RevertButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.SelectedNode != null)
        {
            vm.SelectedNode.RevertCommand.Execute(null);
        }
    }

    private Brush GetThemeBrush(string resourceKey)
    {
        return TryFindResource(resourceKey) as Brush ?? Brushes.Transparent;
    }
}
