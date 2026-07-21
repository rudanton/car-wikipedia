using VehicleKnowledgeManager.Core.Models;

namespace VehicleKnowledgeManager.Core.Interfaces;

public interface IDocumentRepository
{
    Task<IReadOnlyList<DocumentItem>> LoadTreeAsync(
        string rootPath,
        CancellationToken cancellationToken = default);

    Task<string> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        string filePath,
        string content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task MoveAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
