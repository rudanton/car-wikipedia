using CarWikipedia.Core.Models;

namespace CarWikipedia.Core.Interfaces;

public interface IDocxToMarkdownConverter
{
    Task<ConversionResult> ConvertAsync(
        string sourcePath,
        CancellationToken cancellationToken = default);
}
