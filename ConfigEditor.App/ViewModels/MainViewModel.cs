using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigEditor.App.Services;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Core.Models;
using ConfigEditor.Formats;
using ConfigEditor.Infrastructure;

namespace ConfigEditor.App.ViewModels;

/// <summary>
/// Root ViewModel coordinating document views, commands, and application state.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly DialogService _dialogService;
    private readonly RecentFileService _recentFileService;
    private readonly AppSettingsService _appSettingsService;
    private readonly ConfigParserFactory _parserFactory;
    private readonly IBackupService _backupService;
    private readonly IFileService _fileService;
    private readonly LogService _logService;

    [ObservableProperty]
    private ObservableCollection<ConfigDocumentViewModel> _documents = new();

    [ObservableProperty]
    private ConfigDocumentViewModel? _selectedDocument;

    [ObservableProperty]
    private ConfigNodeViewModel? _selectedNode;

    [ObservableProperty]
    private ObservableCollection<ValidationResultViewModel> _validationResults = new();

    [ObservableProperty]
    private ObservableCollection<string> _recentFilesList = new();

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private bool _isLightTheme;

    [ObservableProperty]
    private bool _isInspectingFiles;

    public bool HasUnsavedChanges => Documents.Any(d => d.IsDirty);

    public bool HasDocuments => Documents.Count > 0;

    public bool HasSelectedDocument => SelectedDocument != null;

    public bool HasValidationIssues => ValidationResults.Count > 0;

    public bool HasRecentFiles => RecentFilesList.Count > 0;

    public MainViewModel(
        DialogService dialogService,
        RecentFileService recentFileService,
        AppSettingsService appSettingsService,
        ConfigParserFactory parserFactory,
        IBackupService backupService,
        IFileService fileService,
        LogService logService)
    {
        _dialogService = dialogService;
        _recentFileService = recentFileService;
        _appSettingsService = appSettingsService;
        _parserFactory = parserFactory;
        _backupService = backupService;
        _fileService = fileService;
        _logService = logService;

        _isDarkTheme = _appSettingsService.Settings.Theme.Equals("Dark", StringComparison.OrdinalIgnoreCase);
        _isLightTheme = !_isDarkTheme;

        Documents.CollectionChanged += Documents_CollectionChanged;
        RefreshRecentFiles();
        
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedDocument))
            {
                ValidateActiveDocument();
                SelectedNode = null;
                OnPropertyChanged(nameof(HasSelectedDocument));
            }
        };
    }

    private void Documents_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ConfigDocumentViewModel document in e.NewItems)
            {
                document.PropertyChanged += Document_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ConfigDocumentViewModel document in e.OldItems)
            {
                document.PropertyChanged -= Document_PropertyChanged;
            }
        }

        OnPropertyChanged(nameof(HasDocuments));
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }

    private void Document_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConfigDocumentViewModel.IsDirty))
        {
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }

    [RelayCommand]
    public void ChangeTheme(string themeName)
    {
        if (string.IsNullOrEmpty(themeName)) return;

        IsDarkTheme = themeName.Equals("Dark", StringComparison.OrdinalIgnoreCase);
        IsLightTheme = !IsDarkTheme;

        _appSettingsService.Settings.Theme = IsDarkTheme ? "Dark" : "Light";
        _appSettingsService.SaveSettings();

        if (Application.Current is App app)
        {
            app.ChangeTheme(IsDarkTheme ? "Dark" : "Light");
        }
    }

    private void RefreshRecentFiles()
    {
        RecentFilesList.Clear();
        foreach (var file in _recentFileService.GetRecentFiles())
        {
            RecentFilesList.Add(file);
        }

        OnPropertyChanged(nameof(HasRecentFiles));
    }

    [RelayCommand]
    public void OpenFile(string? filePath = null)
    {
        string filter = "Configuration Files (*.json;*.ini;*.cfg)|*.json;*.ini;*.cfg|JSON Files (*.json)|*.json|INI Files (*.ini;*.cfg)|*.ini;*.cfg|All Files (*.*)|*.*";

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            OpenSingleFile(filePath);
            return;
        }

        var selectedFiles = _dialogService.ShowOpenFileDialogs(filter, _appSettingsService.Settings.LastDirectory);
        foreach (var selectedFile in selectedFiles)
        {
            OpenSingleFile(selectedFile);
        }
    }

    [RelayCommand]
    public async Task InspectFilesAsync()
    {
        string? folderPath = _dialogService.ShowOpenFolderDialog(_appSettingsService.Settings.LastDirectory);
        if (string.IsNullOrWhiteSpace(folderPath))
            return;

        IsInspectingFiles = true;
        try
        {
            _appSettingsService.Settings.LastDirectory = folderPath;
            _appSettingsService.SaveSettings();

            var openableFiles = await Task.Run(() => FindOpenableFiles(folderPath));
            if (openableFiles.Count == 0)
            {
                _dialogService.ShowInfo("선택한 폴더에서 열 수 있는 설정 파일을 찾지 못했습니다.", "파일 조사");
                return;
            }

            int openedCount = 0;
            int failedCount = 0;
            foreach (var filePath in openableFiles)
            {
                if (OpenSingleFile(filePath, showErrors: false))
                {
                    openedCount++;
                }
                else
                {
                    failedCount++;
                }

                await Task.Yield();
            }

            if (failedCount > 0)
            {
                _dialogService.ShowWarning(
                    $"파일 조사를 완료했습니다. 열린 파일: {openedCount}개, 열기 실패: {failedCount}개",
                    "파일 조사 완료");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"파일 조사 중 오류가 발생했습니다: {ex.Message}", "파일 조사 실패");
            _logService.LogError($"Exception inspecting folder {folderPath}", ex);
        }
        finally
        {
            IsInspectingFiles = false;
        }
    }

    private static IReadOnlyList<string> FindOpenableFiles(string folderPath)
    {
        var supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".json",
            ".ini",
            ".cfg"
        };

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true
        };

        return Directory
            .EnumerateFiles(folderPath, "*.*", options)
            .Where(file => supportedExtensions.Contains(Path.GetExtension(file)))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private bool OpenSingleFile(string filePath, bool showErrors = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        string? dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            _appSettingsService.Settings.LastDirectory = dir;
            _appSettingsService.SaveSettings();
        }

        var existing = Documents.FirstOrDefault(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            SelectedDocument = existing;
            _recentFileService.AddFile(filePath);
            RefreshRecentFiles();
            return true;
        }

        try
        {
            _logService.LogInfo($"Opening file: {filePath}");
            var readResult = _fileService.ReadAllText(filePath);
            if (readResult.IsFailure)
            {
                string errorMessage = readResult.Error?.Message ?? "파일을 읽을 수 없습니다.";
                if (showErrors)
                {
                    _dialogService.ShowError(errorMessage, "파일 열기 실패");
                }
                _logService.LogError($"Failed to read file: {filePath}. Error: {errorMessage}");
                if (!File.Exists(filePath))
                {
                    _recentFileService.RemoveFile(filePath);
                    RefreshRecentFiles();
                }
                return false;
            }

            var parser = _parserFactory.GetParser(filePath);
            var doc = parser.Load(filePath);

            var docVm = new ConfigDocumentViewModel(doc, _parserFactory);
            Documents.Add(docVm);
            SelectedDocument = docVm;

            _recentFileService.AddFile(filePath);
            RefreshRecentFiles();
            _logService.LogInfo($"Successfully loaded file: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            if (showErrors)
            {
                _dialogService.ShowError($"파일을 해석하지 못했습니다: {ex.Message}", "오류");
            }
            _logService.LogError($"Exception parsing file {filePath}", ex);
            return false;
        }
    }

    [RelayCommand]
    public void Save()
    {
        if (SelectedDocument == null)
            return;

        ValidateActiveDocument();

        if (ValidationResults.Any(r => r.Severity == ValidationSeverity.Error))
        {
            _dialogService.ShowError("검사 오류가 있는 설정은 저장할 수 없습니다. 오류를 먼저 수정하세요.", "저장 차단");
            return;
        }

        var permissionResult = _fileService.CheckWritePermission(SelectedDocument.FilePath);
        if (permissionResult.IsFailure)
        {
            _dialogService.ShowError(permissionResult.Error!.Message, "쓰기 오류");
            return;
        }

        if (_appSettingsService.Settings.EnableBackup)
        {
            _logService.LogInfo($"Creating backup for: {SelectedDocument.FilePath}");
            var backupResult = _backupService.CreateBackup(SelectedDocument.FilePath);
            if (backupResult.IsFailure)
            {
                _dialogService.ShowError($"백업 생성에 실패했습니다: {backupResult.Error!.Message}. 저장을 중단했습니다.", "백업 실패");
                _logService.LogError($"Backup failed for: {SelectedDocument.FilePath}. Error: {backupResult.Error.Message}");
                return;
            }
            _logService.LogInfo($"Backup created at: {backupResult.Value}");
        }

        try
        {
            _logService.LogInfo($"Saving file: {SelectedDocument.FilePath}");
            
            SelectedDocument.SyncFromTreeToText();

            var writeResult = _fileService.WriteAllText(SelectedDocument.FilePath, SelectedDocument.CurrentText);
            if (writeResult.IsFailure)
            {
                _dialogService.ShowError(writeResult.Error!.Message, "저장 실패");
                _logService.LogError($"Write failed: {SelectedDocument.FilePath}. Error: {writeResult.Error.Message}");
                return;
            }

            SelectedDocument.ResetDirty();
            _logService.LogInfo($"Successfully saved: {SelectedDocument.FilePath}");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"저장 중 오류가 발생했습니다: {ex.Message}", "오류");
            _logService.LogError($"Exception saving {SelectedDocument.FilePath}", ex);
        }
    }

    [RelayCommand]
    public void SaveAs()
    {
        if (SelectedDocument == null)
            return;

        string filter = SelectedDocument.Format == ConfigFormat.Json 
            ? "JSON Files (*.json)|*.json|All Files (*.*)|*.*" 
            : "INI Files (*.ini;*.cfg)|*.ini;*.cfg|All Files (*.*)|*.*";

        string? newFilePath = _dialogService.ShowSaveFileDialog(filter, SelectedDocument.FileName, _appSettingsService.Settings.LastDirectory);
        if (string.IsNullOrEmpty(newFilePath))
            return;

        try
        {
            SelectedDocument.SyncFromTreeToText();
            var writeResult = _fileService.WriteAllText(newFilePath, SelectedDocument.CurrentText);
            if (writeResult.IsFailure)
            {
                _dialogService.ShowError(writeResult.Error!.Message, "다른 이름으로 저장 실패");
                return;
            }

            SelectedDocument.FilePath = newFilePath;
            SelectedDocument.ResetDirty();
            
            _recentFileService.AddFile(newFilePath);
            RefreshRecentFiles();
            _logService.LogInfo($"Successfully saved as: {newFilePath}");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"다른 이름으로 저장 중 오류가 발생했습니다: {ex.Message}", "오류");
            _logService.LogError($"Exception saving as {newFilePath}", ex);
        }
    }

    [RelayCommand]
    public void ValidateActiveDocument()
    {
        ValidationResults.Clear();
        OnPropertyChanged(nameof(HasValidationIssues));
        if (SelectedDocument == null)
            return;

        try
        {
            var validator = _parserFactory.GetValidator(SelectedDocument.FilePath);
            var results = validator.Validate(SelectedDocument.Model);

            foreach (var result in results)
            {
                ValidationResults.Add(new ValidationResultViewModel(result));
            }
        }
        catch (Exception ex)
        {
            _logService.LogError($"Exception validating document {SelectedDocument.FilePath}", ex);
        }
        finally
        {
            OnPropertyChanged(nameof(HasValidationIssues));
        }
    }

    [RelayCommand]
    public void CloseFile(ConfigDocumentViewModel? doc = null)
    {
        doc ??= SelectedDocument;
        if (doc == null)
            return;

        if (doc.IsDirty)
        {
            var res = _dialogService.ShowMessageBox(
                $"'{doc.FileName}' 파일에 저장하지 않은 변경 사항이 있습니다. 닫기 전에 저장하시겠습니까?",
                "저장되지 않은 변경 사항",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            );

            if (res == MessageBoxResult.Yes)
            {
                SelectedDocument = doc;
                Save();
                if (doc.IsDirty) return;
            }
            else if (res == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        Documents.Remove(doc);
        if (SelectedDocument == doc)
        {
            SelectedDocument = Documents.LastOrDefault();
        }
    }

    [RelayCommand]
    public void CreateBackupManual()
    {
        if (SelectedDocument == null)
            return;

        var result = _backupService.CreateBackup(SelectedDocument.FilePath);
        if (result.IsSuccess)
        {
            _dialogService.ShowInfo($"백업을 만들었습니다: {result.Value}", "백업 완료");
        }
        else
        {
            _dialogService.ShowError($"백업 생성에 실패했습니다: {result.Error!.Message}", "백업 실패");
        }
    }

    [RelayCommand]
    public void OpenBackupFolder()
    {
        if (SelectedDocument == null)
            return;

        string? dir = Path.GetDirectoryName(SelectedDocument.FilePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();

        string backupDir = Path.Combine(dir, ".backup");
        _dialogService.OpenFolder(backupDir);
    }

    [RelayCommand]
    public void OpenSelectedFileFolder()
    {
        if (SelectedDocument == null)
            return;

        string? dir = Path.GetDirectoryName(SelectedDocument.FilePath);
        if (string.IsNullOrEmpty(dir))
            return;

        _dialogService.OpenFolder(dir);
    }
}
