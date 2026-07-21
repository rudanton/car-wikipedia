using System.Text;
using CarWikipedia.Core.Interfaces;
using CarWikipedia.Core.Models;

namespace CarWikipedia.Infrastructure.FileSystem;

public sealed class FileDocumentRepository : IDocumentRepository
{
    public Task<IReadOnlyList<DocumentItem>> LoadTreeAsync(
        string rootPath,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"저장소 폴더를 찾을 수 없습니다: {rootPath}");
        }

        var root = new DirectoryInfo(rootPath);
        var children = LoadChildren(root, root.FullName, cancellationToken);
        return Task.FromResult<IReadOnlyList<DocumentItem>>(children);
    }

    public async Task<string> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateMarkdownPath(filePath);
        return await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
    }

    public async Task SaveAsync(
        string filePath,
        string content,
        CancellationToken cancellationToken = default)
    {
        ValidateMarkdownPath(filePath);
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8, cancellationToken);
        File.Move(tempPath, filePath, overwrite: true);
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateMarkdownPath(filePath);
        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task MoveAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ValidateMarkdownPath(sourcePath);
        ValidateMarkdownPath(destinationPath);
        cancellationToken.ThrowIfCancellationRequested();

        var directoryPath = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.Move(sourcePath, destinationPath, overwrite: false);
        return Task.CompletedTask;
    }

    private static List<DocumentItem> LoadChildren(
        DirectoryInfo directory,
        string rootPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directories = directory.EnumerateDirectories()
            .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new DocumentItem
            {
                Name = item.Name,
                FullPath = item.FullName,
                RelativePath = Path.GetRelativePath(rootPath, item.FullName),
                IsDirectory = true,
                Children = LoadChildren(item, rootPath, cancellationToken)
            });

        var files = directory.EnumerateFiles("*.md")
            .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new DocumentItem
            {
                Name = Path.GetFileNameWithoutExtension(item.Name),
                FullPath = item.FullName,
                RelativePath = Path.GetRelativePath(rootPath, item.FullName),
                IsDirectory = false
            });

        return directories.Concat(files).ToList();
    }

    private static void ValidateMarkdownPath(string filePath)
    {
        if (!string.Equals(Path.GetExtension(filePath), ".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Markdown 문서(.md)만 처리할 수 있습니다.");
        }
    }
}
