using VehicleKnowledgeManager.Core.Interfaces;
using VehicleKnowledgeManager.Core.Models;

namespace VehicleKnowledgeManager.Infrastructure.Docx;

public sealed class PlaceholderDocxToMarkdownConverter : IDocxToMarkdownConverter
{
    public Task<ConversionResult> ConvertAsync(
        string sourcePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(sourcePath))
        {
            return Task.FromResult(new ConversionResult
            {
                SourcePath = sourcePath,
                IsSuccess = false,
                ErrorMessage = $"DOCX 파일을 찾을 수 없습니다: {sourcePath}"
            });
        }

        if (!string.Equals(Path.GetExtension(sourcePath), ".docx", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ConversionResult
            {
                SourcePath = sourcePath,
                IsSuccess = false,
                ErrorMessage = "DOCX 파일만 변환할 수 있습니다."
            });
        }

        return Task.FromResult(new ConversionResult
        {
            SourcePath = sourcePath,
            IsSuccess = false,
            ErrorMessage = "DOCX 변환기는 아직 연결되지 않았습니다. Mammoth/ReverseMarkdown 패키지 도입 단계에서 구현합니다."
        });
    }
}
