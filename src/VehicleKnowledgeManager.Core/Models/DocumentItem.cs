namespace VehicleKnowledgeManager.Core.Models;

public sealed class DocumentItem
{
    public required string Name { get; init; }

    public required string FullPath { get; init; }

    public required string RelativePath { get; init; }

    public bool IsDirectory { get; init; }

    public List<DocumentItem> Children { get; init; } = [];
}
