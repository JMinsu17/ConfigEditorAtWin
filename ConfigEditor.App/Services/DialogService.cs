using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace ConfigEditor.App.Services;

/// <summary>
/// Service wrapper for common Windows shell dialogs and message boxes.
/// </summary>
public class DialogService
{
    public string? ShowOpenFileDialog(string filter, string initialDirectory = "")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = Directory.Exists(initialDirectory) ? initialDirectory : ""
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }
        return null;
    }

    public IReadOnlyList<string> ShowOpenFileDialogs(string filter, string initialDirectory = "")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = Directory.Exists(initialDirectory) ? initialDirectory : "",
            Multiselect = true
        };

        return dialog.ShowDialog() == true
            ? dialog.FileNames
            : Array.Empty<string>();
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName = "", string initialDirectory = "")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName,
            InitialDirectory = Directory.Exists(initialDirectory) ? initialDirectory : ""
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }
        return null;
    }

    public string? ShowOpenFolderDialog(string initialDirectory = "")
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = Directory.Exists(initialDirectory) ? initialDirectory : "",
            Multiselect = false
        };

        return dialog.ShowDialog() == true
            ? dialog.FolderName
            : null;
    }

    public MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
    {
        return MessageBox.Show(message, title, button, image);
    }

    public void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowInfo(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true
            });
        }
        else
        {
            ShowError($"Folder not found: {folderPath}", "Error");
        }
    }
}
