namespace VehicleKnowledgeManager.Core.Models;

public sealed class SearchResult
{
    public required string FileName { get; init; }

    public required string FullPath { get; init; }

    public required string RelativePath { get; init; }

    public required string PreviewText { get; init; }

    public int MatchCount { get; init; }
}
