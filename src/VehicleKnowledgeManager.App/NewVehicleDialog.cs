using System.Windows;
using System.Windows.Controls;

namespace CarWikipedia.App;

public sealed class NewVehicleDialog : Window
{
    private readonly TextBox _manufacturerBox = new();
    private readonly TextBox _vehicleNameBox = new();

    public NewVehicleDialog()
    {
        Title = "새 차량 문서";
        Width = 360;
        Height = 210;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var panel = new Grid
        {
            Margin = new Thickness(16)
        };
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        AddLabel(panel, "제조사", 0);
        AddTextBox(panel, _manufacturerBox, 1);
        AddLabel(panel, "차량명", 2);
        AddTextBox(panel, _vehicleNameBox, 3);

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

        var cancelButton = new Button
        {
            Content = "취소",
            IsCancel = true,
            MinWidth = 72
        };

        buttons.Children.Add(createButton);
        buttons.Children.Add(cancelButton);
        Grid.SetRow(buttons, 4);
        panel.Children.Add(buttons);

        Content = panel;
    }

    public string ManufacturerName => _manufacturerBox.Text.Trim();

    public string VehicleName => _vehicleNameBox.Text.Trim();

    private static void AddLabel(Grid panel, string text, int row)
    {
        var label = new TextBlock
        {
            Text = text,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, row == 0 ? 0 : 10, 0, 4)
        };
        Grid.SetRow(label, row);
        panel.Children.Add(label);
    }

    private static void AddTextBox(Grid panel, TextBox textBox, int row)
    {
        textBox.Height = 28;
        textBox.VerticalContentAlignment = VerticalAlignment.Center;
        Grid.SetRow(textBox, row);
        panel.Children.Add(textBox);
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ManufacturerName) || string.IsNullOrWhiteSpace(VehicleName))
        {
            MessageBox.Show(this, "제조사와 차량명을 모두 입력하세요.", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }
}
