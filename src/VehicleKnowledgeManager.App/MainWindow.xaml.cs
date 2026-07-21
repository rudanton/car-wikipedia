using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CarWikipedia.Core.Models;
using CarWikipedia.Core.Settings;
using CarWikipedia.Infrastructure.FileSystem;
using CarWikipedia.Infrastructure.Settings;
using Microsoft.Win32;

namespace CarWikipedia.App;

public partial class MainWindow : Window
{
    private readonly FileDocumentRepository _documentRepository = new();
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
            var markdown = await _documentRepository.ReadAsync(document.FullPath);
            _currentDocument = document;
            _currentTreeItem = treeViewItem;
            _currentMarkdown = markdown;
            VehicleReadTitle.Text = document.Name;
            VehicleReadDocumentViewer.Document = RenderMarkdown(markdown);
            MarkdownEditor.Text = markdown;
            SetDirtyState(false);
            SetEditMode(false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            VehicleReadTitle.Text = document.Name;
            VehicleReadDocumentViewer.Document = RenderMarkdown($"문서를 열 수 없습니다.\n\n- {exception.Message}");
        }
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

    private void MarkdownEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_currentDocument is null)
        {
            return;
        }

        SetDirtyState(MarkdownEditor.Text != _currentMarkdown);
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
