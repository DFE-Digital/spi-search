using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.LearningProviders;

namespace Dfe.Spi.Search.Application.LearningProviders
{
    public interface ILearningProviderSearchManager
    {
        Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request,
            CancellationToken cancellationToken);
    }

    public class LearningProviderSearchManager : ILearningProviderSearchManager
    {
        private readonly ILearningProviderSearchIndex _searchIndex;
        private readonly ILoggerWrapper _logger;

        public LearningProviderSearchManager(
            ILearningProviderSearchIndex searchIndex,
            ILoggerWrapper logger)
        {
            _searchIndex = searchIndex;
            _logger = logger;
        }

        public async Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request,
            CancellationToken cancellationToken)
        {
            EnsureSearchRequestIsValid(request);

            return await _searchIndex.SearchAsync(request, cancellationToken);
        }


        
        
        
        private static readonly string[] FieldsValidForFiltering = new[] {"Name"};

        private void EnsureSearchRequestIsValid(SearchRequest request)
        {
            if (request == null)
            {
                throw new InvalidRequestException("Must provide SearchRequest");
            }

            if (request.Filter == null)
            {
                throw new InvalidRequestException("Must provide filters");
            }

            var validationProblems = new List<string>();
            foreach (var filter in request.Filter)
            {
                if (!FieldsValidForFiltering.Any(f => f == filter.Field))
                {
                    validationProblems.Add($"{filter.Field} is not a valid field for filtering");
                }
            }

            if (validationProblems.Count > 0)
            {
                throw new InvalidRequestException(validationProblems.ToArray());
            }
        }
    }
}