using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CarWikipedia.Core.Models;
using CarWikipedia.Core.Settings;
using CarWikipedia.Infrastructure.FileSystem;
using CarWikipedia.Infrastructure.Markdown;
using CarWikipedia.Infrastructure.Settings;
using Microsoft.Win32;

namespace CarWikipedia.App;

public partial class MainWindow : Window
{
    private readonly FileDocumentRepository _documentRepository = new();
    private readonly MarkdownSearchService _searchService = new();
    private readonly JsonAppSettingsService _settingsService = new();
    private readonly AppSettings _settings;
    private DocumentItem? _currentDocument;
    private TreeViewItem? _currentTreeItem;
    private bool _hasUnsavedChanges;
    private string _currentMarkdown = string.Empty;
    private IReadOnlyList<DocumentItem> _documentTree = [];
    private bool _isEditMode;
    private bool _suppressSelectionChange;
    private string? _repositoryPath;

    public MainWindow()
    {
        InitializeComponent();

        _settings = _settingsService.Load();
        _repositoryPath = _settings.LastRepositoryPath;
        RepositoryPathText.Text = string.IsNullOrWhiteSpace(_repositoryPath)
            ? "저장소가 선택되지 않았습니다."
            : _repositoryPath;

        RefreshRecentVehicles();
        RefreshFavoriteVehicles();
        _ = ScanVehicleDocumentsAsync();
    }

    private async void OpenRepositoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "차량 문서 저장소 폴더 선택",
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(_repositoryPath))
        {
            dialog.InitialDirectory = _repositoryPath;
        }

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _repositoryPath = dialog.FolderName;
        _settings.LastRepositoryPath = _repositoryPath;
        _settingsService.Save(_settings);
        RepositoryPathText.Text = _repositoryPath;
        RefreshRecentVehicles();
        RefreshFavoriteVehicles();

        await ScanVehicleDocumentsAsync();
    }

    private async Task ScanVehicleDocumentsAsync()
    {
        if (string.IsNullOrWhiteSpace(_repositoryPath))
        {
            _documentTree = [];
            DocumentScanStatusText.Text = "스캔 대기 중";
            VehicleTree.Items.Clear();
            return;
        }

        var vehicleRootPath = ResolveVehicleRootPath(_repositoryPath);
        if (!Directory.Exists(vehicleRootPath))
        {
            _documentTree = [];
            DocumentScanStatusText.Text = "vehicles 폴더를 찾을 수 없습니다.";
            VehicleTree.Items.Clear();
            return;
        }

        try
        {
            _documentTree = await _documentRepository.LoadTreeAsync(vehicleRootPath);
            DocumentScanStatusText.Text = $"Markdown 문서 {CountDocuments(_documentTree)}개";
            PopulateVehicleTree(_documentTree);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
        {
            _documentTree = [];
            DocumentScanStatusText.Text = $"스캔 실패: {exception.Message}";
            VehicleTree.Items.Clear();
        }
    }

    private async void VehicleTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (_suppressSelectionChange)
        {
            return;
        }

        if (e.NewValue is not TreeViewItem treeViewItem || treeViewItem.Tag is not DocumentItem document)
        {
            return;
        }

        if (document.IsDirectory)
        {
            if (_hasUnsavedChanges)
            {
                RestoreCurrentSelection();
            }

            return;
        }

        if (_hasUnsavedChanges && !ConfirmDiscardChanges())
        {
            RestoreCurrentSelection();
            return;
        }

        try
        {
            _currentTreeItem = treeViewItem;
            await OpenDocumentAsync(document);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            VehicleReadTitle.Text = document.Name;
            VehicleReadDocumentViewer.Document = RenderMarkdown($"문서를 열 수 없습니다.\n\n- {exception.Message}");
        }
    }

    private async void SearchResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SearchResultsList.SelectedItem is not ListBoxItem item || item.Tag is not SearchResult result)
        {
            return;
        }

        if (_hasUnsavedChanges && !ConfirmDiscardChanges())
        {
            SearchResultsList.SelectedItem = null;
            return;
        }

        var document = new DocumentItem
        {
            Name = Path.GetFileNameWithoutExtension(result.FileName),
            FullPath = result.FullPath,
            RelativePath = result.RelativePath,
            IsDirectory = false
        };

        _currentTreeItem = null;
        await OpenDocumentAsync(document);
    }

    private async void RecentVehiclesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RecentVehiclesList.SelectedItem is not ListBoxItem item || item.Tag is not string fullPath)
        {
            return;
        }

        if (_hasUnsavedChanges && !ConfirmDiscardChanges())
        {
            RecentVehiclesList.SelectedItem = null;
            return;
        }

        var document = new DocumentItem
        {
            Name = Path.GetFileNameWithoutExtension(fullPath),
            FullPath = fullPath,
            RelativePath = string.IsNullOrWhiteSpace(_repositoryPath)
                ? Path.GetFileName(fullPath)
                : Path.GetRelativePath(ResolveVehicleRootPath(_repositoryPath), fullPath),
            IsDirectory = false
        };

        _currentTreeItem = null;
        await OpenDocumentAsync(document);
    }

    private async void FavoriteVehiclesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FavoriteVehiclesList.SelectedItem is not ListBoxItem item || item.Tag is not string fullPath)
        {
            return;
        }

        if (_hasUnsavedChanges && !ConfirmDiscardChanges())
        {
            FavoriteVehiclesList.SelectedItem = null;
            return;
        }

        var document = new DocumentItem
        {
            Name = Path.GetFileNameWithoutExtension(fullPath),
            FullPath = fullPath,
            RelativePath = string.IsNullOrWhiteSpace(_repositoryPath)
                ? Path.GetFileName(fullPath)
                : Path.GetRelativePath(ResolveVehicleRootPath(_repositoryPath), fullPath),
            IsDirectory = false
        };

        _currentTreeItem = null;
        await OpenDocumentAsync(document);
    }

    private void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        var fullPath = _currentDocument.FullPath;
        var existingIndex = _settings.FavoriteVehiclePaths.FindIndex(path =>
            string.Equals(path, fullPath, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            _settings.FavoriteVehiclePaths.RemoveAt(existingIndex);
            DocumentScanStatusText.Text = $"즐겨찾기 해제: {_currentDocument.Name}";
        }
        else
        {
            _settings.FavoriteVehiclePaths.Insert(0, fullPath);
            DocumentScanStatusText.Text = $"즐겨찾기 추가: {_currentDocument.Name}";
        }

        _settingsService.Save(_settings);
        RefreshFavoriteVehicles();
        UpdateFavoriteButton();
    }

    private void EditModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        SetEditMode(!_isEditMode);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        try
        {
            _currentMarkdown = MarkdownEditor.Text;
            await _documentRepository.SaveAsync(_currentDocument.FullPath, _currentMarkdown);
            VehicleReadDocumentViewer.Document = RenderMarkdown(_currentMarkdown);
            SetDirtyState(false);
            DocumentScanStatusText.Text = $"저장 완료: {_currentDocument.Name}";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            DocumentScanStatusText.Text = $"저장 실패: {exception.Message}";
        }
    }

    private async void NewDocumentButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_repositoryPath))
        {
            MessageBox.Show(this, "저장소를 먼저 선택하세요.", "저장소 필요", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_hasUnsavedChanges && !ConfirmDiscardChanges())
        {
            return;
        }

        var dialog = new NewVehicleDialog
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var vehicleRootPath = ResolveVehicleRootPath(_repositoryPath);
        var manufacturerPath = Path.Combine(vehicleRootPath, SanitizeFileName(dialog.ManufacturerName));
        var filePath = Path.Combine(manufacturerPath, SanitizeFileName(dialog.VehicleName) + ".md");

        if (File.Exists(filePath))
        {
            MessageBox.Show(this, "같은 이름의 차량 문서가 이미 있습니다.", "문서 생성 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var content = _settings.DefaultDocumentTemplate
            .Replace("차량명", dialog.VehicleName, StringComparison.Ordinal)
            .Replace("트림명", "트림명", StringComparison.Ordinal);

        await _documentRepository.SaveAsync(filePath, content);
        await ScanVehicleDocumentsAsync();

        var document = new DocumentItem
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            FullPath = filePath,
            RelativePath = Path.GetRelativePath(vehicleRootPath, filePath),
            IsDirectory = false
        };
        _currentTreeItem = null;
        await OpenDocumentAsync(document);
        SetEditMode(true);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"'{_currentDocument.Name}' 문서를 삭제할까요?",
            "문서 삭제 확인",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var deletedPath = _currentDocument.FullPath;
        await _documentRepository.DeleteAsync(deletedPath);

        _settings.RecentVehiclePaths.RemoveAll(path => string.Equals(path, deletedPath, StringComparison.OrdinalIgnoreCase));
        _settings.FavoriteVehiclePaths.RemoveAll(path => string.Equals(path, deletedPath, StringComparison.OrdinalIgnoreCase));
        _settingsService.Save(_settings);

        _currentDocument = null;
        _currentTreeItem = null;
        _currentMarkdown = string.Empty;
        MarkdownEditor.Text = string.Empty;
        VehicleReadTitle.Text = "차량 정보";
        VehicleReadDocumentViewer.Document = RenderMarkdown("왼쪽에서 차량 문서를 선택하세요.");
        SetDirtyState(false);
        SetEditMode(false);
        RefreshRecentVehicles();
        RefreshFavoriteVehicles();
        await ScanVehicleDocumentsAsync();
    }

    private async void MoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDocument is null || string.IsNullOrWhiteSpace(_repositoryPath))
        {
            return;
        }

        if (_hasUnsavedChanges && !ConfirmDiscardChanges())
        {
            return;
        }

        var vehicleRootPath = ResolveVehicleRootPath(_repositoryPath);
        var currentManufacturer = GetManufacturerName(_currentDocument.RelativePath);
        var dialog = new MoveVehicleDialog(currentManufacturer)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var destinationDirectory = Path.Combine(vehicleRootPath, SanitizeFileName(dialog.ManufacturerName));
        var destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(_currentDocument.FullPath));

        if (string.Equals(_currentDocument.FullPath, destinationPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(destinationPath))
        {
            MessageBox.Show(this, "이동할 위치에 같은 이름의 문서가 이미 있습니다.", "문서 이동 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sourcePath = _currentDocument.FullPath;
        await _documentRepository.MoveAsync(sourcePath, destinationPath);
        ReplacePath(_settings.RecentVehiclePaths, sourcePath, destinationPath);
        ReplacePath(_settings.FavoriteVehiclePaths, sourcePath, destinationPath);
        _settingsService.Save(_settings);

        await ScanVehicleDocumentsAsync();
        var movedDocument = new DocumentItem
        {
            Name = Path.GetFileNameWithoutExtension(destinationPath),
            FullPath = destinationPath,
            RelativePath = Path.GetRelativePath(vehicleRootPath, destinationPath),
            IsDirectory = false
        };
        _currentTreeItem = null;
        await OpenDocumentAsync(movedDocument);
        DocumentScanStatusText.Text = $"이동 완료: {movedDocument.Name}";
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        return fileName.Trim();
    }

    private static string GetManufacturerName(string relativePath)
    {
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Length > 1 ? parts[0] : string.Empty;
    }

    private static void ReplacePath(List<string> paths, string oldPath, string newPath)
    {
        var index = paths.FindIndex(path => string.Equals(path, oldPath, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            paths[index] = newPath;
        }
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_repositoryPath))
        {
            SearchResultsTitle.Text = "검색 결과";
            SearchResultsList.Items.Clear();
            SearchResultsList.Items.Add("저장소를 먼저 선택하세요.");
            return;
        }

        var keyword = SearchBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            SearchResultsTitle.Text = "검색 결과";
            SearchResultsList.Items.Clear();
            SearchResultsList.Items.Add("검색어를 입력하세요.");
            return;
        }

        var vehicleRootPath = ResolveVehicleRootPath(_repositoryPath);
        var results = await _searchService.SearchAsync(vehicleRootPath, keyword);
        SearchResultsTitle.Text = $"검색 결과 {results.Count}개";
        SearchResultsList.Items.Clear();

        foreach (var result in results)
        {
            SearchResultsList.Items.Add(CreateSearchResultItem(result));
        }
    }

    private void MarkdownEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        SetDirtyState(MarkdownEditor.Text != _currentMarkdown);
    }

    private async Task OpenDocumentAsync(DocumentItem document)
    {
        var markdown = await _documentRepository.ReadAsync(document.FullPath);
        _currentDocument = document;
        _currentMarkdown = markdown;
        VehicleReadTitle.Text = document.Name;
        VehicleReadDocumentViewer.Document = RenderMarkdown(markdown);
        MarkdownEditor.Text = markdown;
        SetDirtyState(false);
        SetEditMode(false);
        RememberRecentVehicle(document.FullPath);
        UpdateFavoriteButton();
    }

    private static string ResolveVehicleRootPath(string repositoryPath)
    {
        var vehiclesPath = Path.Combine(repositoryPath, "vehicles");
        return Directory.Exists(vehiclesPath) ? vehiclesPath : repositoryPath;
    }

    private static int CountDocuments(IEnumerable<DocumentItem> items)
    {
        return items.Sum(item => item.IsDirectory ? CountDocuments(item.Children) : 1);
    }

    private static FlowDocument RenderMarkdown(string markdown)
    {
        var document = new FlowDocument
        {
            FontSize = 15,
            LineHeight = 24,
            PagePadding = new Thickness(0)
        };

        foreach (var rawLine in markdown.ReplaceLineEndings("\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            document.Blocks.Add(CreateReadBlock(line));
        }

        if (document.Blocks.Count == 0)
        {
            document.Blocks.Add(new Paragraph(new Run("내용이 없습니다.")));
        }

        return document;
    }

    private static Block CreateReadBlock(string line)
    {
        if (line.StartsWith("### ", StringComparison.Ordinal))
        {
            return new Paragraph(new Run(line[4..]))
            {
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16)
            };
        }

        if (line.StartsWith("# ", StringComparison.Ordinal))
        {
            return new Paragraph(new Run(line[2..]))
            {
                FontSize = 21,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 18, 0, 8)
            };
        }

        if (line.StartsWith("- ", StringComparison.Ordinal))
        {
            return new Paragraph(new Run($"• {line[2..]}"))
            {
                Margin = new Thickness(18, 2, 0, 2)
            };
        }

        if (line is "기본 옵션" or "추가 옵션")
        {
            return new Paragraph(new Run(line))
            {
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.DarkSlateGray,
                Margin = new Thickness(0, 8, 0, 4)
            };
        }

        return new Paragraph(new Run(line))
        {
            Margin = new Thickness(0, 2, 0, 2)
        };
    }

    private void PopulateVehicleTree(IEnumerable<DocumentItem> items)
    {
        VehicleTree.Items.Clear();
        _currentTreeItem = null;

        foreach (var item in items)
        {
            VehicleTree.Items.Add(CreateTreeItem(item));
        }
    }

    private static TreeViewItem CreateTreeItem(DocumentItem item)
    {
        var treeItem = new TreeViewItem
        {
            Header = item.Name,
            Tag = item
        };

        foreach (var child in item.Children)
        {
            treeItem.Items.Add(CreateTreeItem(child));
        }

        return treeItem;
    }

    private static ListBoxItem CreateSearchResultItem(SearchResult result)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(4)
        };

        panel.Children.Add(new TextBlock
        {
            Text = $"{result.FileName} ({result.MatchCount})",
            FontWeight = FontWeights.SemiBold
        });

        panel.Children.Add(new TextBlock
        {
            Text = result.RelativePath,
            Foreground = Brushes.DimGray,
            FontSize = 12
        });

        if (!string.IsNullOrWhiteSpace(result.PreviewText))
        {
            panel.Children.Add(new TextBlock
            {
                Text = result.PreviewText,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            });
        }

        return new ListBoxItem
        {
            Content = panel,
            Tag = result
        };
    }

    private void RememberRecentVehicle(string fullPath)
    {
        _settings.RecentVehiclePaths.RemoveAll(path => string.Equals(path, fullPath, StringComparison.OrdinalIgnoreCase));
        _settings.RecentVehiclePaths.Insert(0, fullPath);

        if (_settings.RecentVehiclePaths.Count > 10)
        {
            _settings.RecentVehiclePaths.RemoveRange(10, _settings.RecentVehiclePaths.Count - 10);
        }

        _settingsService.Save(_settings);
        RefreshRecentVehicles();
    }

    private void RefreshRecentVehicles()
    {
        RecentVehiclesList.Items.Clear();

        foreach (var fullPath in _settings.RecentVehiclePaths.Where(File.Exists))
        {
            RecentVehiclesList.Items.Add(new ListBoxItem
            {
                Content = Path.GetFileNameWithoutExtension(fullPath),
                Tag = fullPath
            });
        }
    }

    private void RefreshFavoriteVehicles()
    {
        FavoriteVehiclesList.Items.Clear();

        foreach (var fullPath in _settings.FavoriteVehiclePaths.Where(File.Exists))
        {
            FavoriteVehiclesList.Items.Add(new ListBoxItem
            {
                Content = Path.GetFileNameWithoutExtension(fullPath),
                Tag = fullPath
            });
        }
    }

    private void UpdateFavoriteButton()
    {
        if (_currentDocument is null)
        {
            FavoriteButton.Content = "즐겨찾기";
            return;
        }

        var isFavorite = _settings.FavoriteVehiclePaths.Any(path =>
            string.Equals(path, _currentDocument.FullPath, StringComparison.OrdinalIgnoreCase));
        FavoriteButton.Content = isFavorite ? "즐겨찾기 해제" : "즐겨찾기";
    }

    private void SetEditMode(bool isEditMode)
    {
        _isEditMode = isEditMode;
        MarkdownEditor.Visibility = isEditMode ? Visibility.Visible : Visibility.Collapsed;
        VehicleReadScroll.Visibility = isEditMode ? Visibility.Collapsed : Visibility.Visible;
        EditModeButton.Content = isEditMode ? "읽기" : "편집";
        VehicleReadTitle.Text = _currentDocument is null
            ? "차량 정보"
            : BuildDocumentTitle(isEditMode);
    }

    private void SetDirtyState(bool hasUnsavedChanges)
    {
        _hasUnsavedChanges = hasUnsavedChanges;
        VehicleReadTitle.Text = _currentDocument is null
            ? "차량 정보"
            : BuildDocumentTitle(_isEditMode);

        if (_currentDocument is not null && hasUnsavedChanges)
        {
            DocumentScanStatusText.Text = $"저장되지 않은 변경: {_currentDocument.Name}";
        }
    }

    private string BuildDocumentTitle(bool isEditMode)
    {
        if (_currentDocument is null)
        {
            return "차량 정보";
        }

        var suffix = _hasUnsavedChanges ? " *" : string.Empty;
        return isEditMode ? $"{_currentDocument.Name} 편집{suffix}" : $"{_currentDocument.Name}{suffix}";
    }

    private bool ConfirmDiscardChanges()
    {
        var result = MessageBox.Show(
            this,
            "저장하지 않은 변경이 있습니다. 변경 내용을 버리고 다른 문서를 열까요?",
            "변경 내용 확인",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }

    private void RestoreCurrentSelection()
    {
        if (_currentTreeItem is null)
        {
            return;
        }

        _suppressSelectionChange = true;
        _currentTreeItem.IsSelected = true;
        _suppressSelectionChange = false;
    }
}
