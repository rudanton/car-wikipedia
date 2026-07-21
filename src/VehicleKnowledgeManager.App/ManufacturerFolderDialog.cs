using System.Windows;
using System.Windows.Controls;

namespace CarWikipedia.App;

public sealed class ManufacturerFolderDialog : Window
{
    private readonly TextBox _manufacturerBox = new();

    public ManufacturerFolderDialog()
    {
        Title = "새 제조사 폴더";
        Width = 340;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var panel = new StackPanel
        {
            Margin = new Thickness(16)
        };

        panel.Children.Add(new TextBlock
        {
            Text = "제조사 폴더명",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 4)
        });

        _manufacturerBox.Height = 28;
        _manufacturerBox.VerticalContentAlignment = VerticalAlignment.Center;
        panel.Children.Add(_manufacturerBox);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 14, 0, 0)
        };

        var createButton = new Button
        {
            Content = "생성",
            IsDefault = true,
            MinWidth = 72,
            Margin = new Thickness(0, 0, 8, 0)
        };
        createButton.Click += CreateButton_Click;

        buttons.Children.Add(createButton);
        buttons.Children.Add(new Button
        {
            Content = "취소",
            IsCancel = true,
            MinWidth = 72
        });

        panel.Children.Add(buttons);
        Content = panel;
    }

    public string ManufacturerName => _manufacturerBox.Text.Trim();

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ManufacturerName))
        {
            MessageBox.Show(this, "제조사 폴더명을 입력하세요.", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }
}
