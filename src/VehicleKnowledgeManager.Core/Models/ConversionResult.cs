namespace VehicleKnowledgeManager.Core.Models;

public sealed class ConversionResult
{
    public required string SourcePath { get; init; }

    public string? OutputPath { get; init; }

    public bool IsSuccess { get; init; }

    public string? Markdown { get; init; }

    public string? ErrorMessage { get; init; }

    public List<string> Warnings { get; init; } = [];
}
