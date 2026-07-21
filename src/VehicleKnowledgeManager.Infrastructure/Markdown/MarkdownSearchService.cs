using System.Text;
using CarWikipedia.Core.Interfaces;
using CarWikipedia.Core.Models;

namespace CarWikipedia.Infrastructure.Markdown;

public sealed class MarkdownSearchService : ISearchService
{
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        string rootPath,
        string keyword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"저장소 폴더를 찾을 수 없습니다: {rootPath}");
        }

        var results = new List<SearchResult>();
        var comparison = StringComparison.CurrentCultureIgnoreCase;

        foreach (var filePath in Directory.EnumerateFiles(rootPath, "*.md", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
            var matchCount = CountMatches(text, keyword, comparison);
            var fileNameMatches = Path.GetFileName(filePath).Contains(keyword, comparison);
            var relativeDirectory = Path.GetDirectoryName(Path.GetRelativePath(rootPath, filePath));
            var folderMatches = relativeDirectory?.Contains(keyword, comparison) == true;

            if (matchCount == 0 && !fileNameMatches && !folderMatches)
            {
                continue;
            }

            results.Add(new SearchResult
            {
                FileName = Path.GetFileName(filePath),
                FullPath = filePath,
                RelativePath = Path.GetRelativePath(rootPath, filePath),
                PreviewText = BuildPreview(text, keyword, comparison),
                MatchCount = matchCount + (fileNameMatches ? 1 : 0) + (folderMatches ? 1 : 0)
            });
        }

        return results
            .OrderByDescending(item => item.MatchCount)
            .ThenBy(item => item.RelativePath, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static int CountMatches(string text, string keyword, StringComparison comparison)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(keyword, index, comparison)) >= 0)
        {
            count++;
            index += keyword.Length;
        }

        return count;
    }

    private static string BuildPreview(string text, string keyword, StringComparison comparison)
    {
        var index = text.IndexOf(keyword, comparison);
        if (index < 0)
        {
            return string.Empty;
        }

        var start = Math.Max(0, index - 60);
        var length = Math.Min(text.Length - start, keyword.Length + 120);
        return text.Substring(start, length).ReplaceLineEndings(" ").Trim();
    }
}
