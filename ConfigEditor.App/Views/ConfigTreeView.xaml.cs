using System.Windows;
using System.Windows.Controls;
using ConfigEditor.App.ViewModels;

namespace ConfigEditor.App.Views;

/// <summary>
/// Interaction logic for ConfigTreeView.xaml
/// </summary>
public partial class ConfigTreeView : UserControl
{
    public ConfigTreeView()
    {
        InitializeComponent();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel mainVm)
        {
            mainVm.SelectedNode = e.NewValue as ConfigNodeViewModel;
        }
    }
}
