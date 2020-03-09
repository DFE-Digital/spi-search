using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Spi.Search.Domain.Common
{
    public interface ISearchIndex<T> where T : SearchDocument
    {
        Task<string[]> GetSearchableFieldsAsync(CancellationToken cancellationToken);
        Task UploadBatchAsync(T[] documents, CancellationToken cancellationToken);
        Task<SearchResultset<T>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    }
}