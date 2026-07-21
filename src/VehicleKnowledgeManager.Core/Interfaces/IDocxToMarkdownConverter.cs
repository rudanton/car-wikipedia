using VehicleKnowledgeManager.Core.Models;

namespace VehicleKnowledgeManager.Core.Interfaces;

public interface IDocxToMarkdownConverter
{
    Task<ConversionResult> ConvertAsync(
        string sourcePath,
        CancellationToken cancellationToken = default);
}
