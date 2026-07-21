using VehicleKnowledgeManager.Core.Models;

namespace VehicleKnowledgeManager.Core.Interfaces;

public interface ISearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        string rootPath,
        string keyword,
        CancellationToken cancellationToken = default);
}
