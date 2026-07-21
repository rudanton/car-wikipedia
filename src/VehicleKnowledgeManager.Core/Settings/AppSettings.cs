namespace CarWikipedia.Core.Settings;

public sealed class AppSettings
{
    public string? LastRepositoryPath { get; set; }

    public double EditorFontSize { get; set; } = 14;

    public bool AutoSaveEnabled { get; set; }

    public bool PreviewEnabled { get; set; } = true;

    public List<string> RecentVehiclePaths { get; set; } = [];

    public List<string> FavoriteVehiclePaths { get; set; } = [];

    public string DefaultDocumentTemplate { get; set; } = """
        ### 차량명

        # 트림명
        기본 옵션
        -

        추가 옵션
        -
        """;
}
