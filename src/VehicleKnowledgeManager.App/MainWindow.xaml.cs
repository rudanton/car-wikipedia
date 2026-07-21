using System.Windows;
using Microsoft.Win32;

namespace CarWikipedia.App;

public partial class MainWindow : Window
{
    private string? _repositoryPath;

    public MainWindow()
    {
        InitializeComponent();
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
        RepositoryPathText.Text = _repositoryPath;
    }
}
