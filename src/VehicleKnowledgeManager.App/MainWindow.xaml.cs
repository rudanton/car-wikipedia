using System.IO;
using System.Windows;
using System.Windows.Controls;
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
    private IReadOnlyList<DocumentItem> _documentTree = [];
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
        if (e.NewValue is not TreeViewItem treeViewItem || treeViewItem.Tag is not DocumentItem document)
        {
            return;
        }

        if (document.IsDirectory)
        {
            return;
        }

        try
        {
            var markdown = await _documentRepository.ReadAsync(document.FullPath);
            VehicleReadTitle.Text = document.Name;
            VehicleReadText.Text = ToReadText(markdown);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            VehicleReadTitle.Text = document.Name;
            VehicleReadText.Text = $"문서를 열 수 없습니다.\n\n{exception.Message}";
        }
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

    private static string ToReadText(string markdown)
    {
        var lines = markdown.ReplaceLineEndings("\n")
            .Split('\n')
            .Select(line => line.TrimEnd())
            .Select(line => line.StartsWith("### ", StringComparison.Ordinal) ? line[4..] : line)
            .Select(line => line.StartsWith("# ", StringComparison.Ordinal) ? $"\n{line[2..]}" : line)
            .Select(line => line.StartsWith("- ", StringComparison.Ordinal) ? $"  • {line[2..]}" : line);

        return string.Join(Environment.NewLine, lines).Trim();
    }

    private void PopulateVehicleTree(IEnumerable<DocumentItem> items)
    {
        VehicleTree.Items.Clear();

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
}
