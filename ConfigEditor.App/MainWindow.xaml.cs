using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ConfigEditor.App.ViewModels;

namespace ConfigEditor.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source == MainTabControl)
        {
            var vm = DataContext as MainViewModel;
            if (vm?.SelectedDocument == null) return;

            if (MainTabControl.SelectedItem == FormTab)
            {
                bool parsed = vm.SelectedDocument.SyncFromTextToTree();
                if (!parsed)
                {
                    MessageBox.Show("원문에 문법 오류가 있어 폼 편집기로 전환할 수 없습니다. 원문 편집에서 오류를 먼저 수정하세요.", "문법 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    MainTabControl.SelectedItem = RawTextTab;
                }
                else
                {
                    vm.ValidateActiveDocument();
                }
            }
            else if (MainTabControl.SelectedItem == RawTextTab)
            {
                vm.SelectedDocument.SyncFromTreeToText();
            }
        }
    }

    private void RecentFileMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { DataContext: string filePath } && DataContext is MainViewModel vm)
        {
            vm.OpenFile(filePath);
            e.Handled = true;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        var vm = DataContext as MainViewModel;
        if (vm != null && vm.HasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "저장하지 않은 변경 사항이 있습니다. 종료하기 전에 저장하시겠습니까?",
                "저장되지 않은 변경 사항",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                foreach (var doc in vm.Documents.ToList())
                {
                    vm.SelectedDocument = doc;
                    vm.Save();
                }

                if (vm.HasUnsavedChanges)
                {
                    e.Cancel = true;
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        base.OnClosing(e);
    }
}
