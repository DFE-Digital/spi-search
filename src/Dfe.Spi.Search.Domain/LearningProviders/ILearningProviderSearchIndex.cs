using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Search.Domain.Common;

namespace Dfe.Spi.Search.Domain.LearningProviders
{
    public interface ILearningProviderSearchIndex
    {
        Task<string[]> GetSearchableFieldsAsync(CancellationToken cancellationToken);
        Task CreateOrUpdateIndexAsync(CancellationToken cancellationToken);
        Task UploadBatchAsync(LearningProviderSearchDocument[] documents, CancellationToken cancellationToken);
        Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
    }
}