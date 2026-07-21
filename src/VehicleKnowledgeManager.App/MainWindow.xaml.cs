using System.Windows;
using CarWikipedia.Core.Settings;
using CarWikipedia.Infrastructure.Settings;
using Microsoft.Win32;

namespace CarWikipedia.App;

public partial class MainWindow : Window
{
    private readonly JsonAppSettingsService _settingsService = new();
    private readonly AppSettings _settings;
    private string? _repositoryPath;

    public MainWindow()
    {
        InitializeComponent();

        _settings = _settingsService.Load();
        _repositoryPath = _settings.LastRepositoryPath;
        RepositoryPathText.Text = string.IsNullOrWhiteSpace(_repositoryPath)
            ? "저장소가 선택되지 않았습니다."
            : _repositoryPath;
    }

    private void OpenRepositoryButton_Click(object sender, RoutedEventArgs e)
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
    }
}
