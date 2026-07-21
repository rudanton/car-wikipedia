using CarWikipedia.Core.Models;

namespace CarWikipedia.Core.Interfaces;

public interface ISearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        string rootPath,
        string keyword,
        CancellationToken cancellationToken = default);
}
