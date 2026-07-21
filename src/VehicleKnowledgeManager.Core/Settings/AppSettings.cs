namespace CarWikipedia.Core.Settings;

public sealed class AppSettings
{
    public string? LastRepositoryPath { get; set; }

    public double EditorFontSize { get; set; } = 14;

    public bool AutoSaveEnabled { get; set; }

    public bool PreviewEnabled { get; set; } = true;

    public string DefaultDocumentTemplate { get; set; } = """
        # 차량명

        ## 기본 정보

        - 제조사:
        - 차급:
        - 연료:
        - 비고:

        ## 트림

        ### 트림명

        기본 사양

        -

        선택 옵션

        -

        ## 상담 포인트

        -
        """;
}
