using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.LearningProviders;

namespace Dfe.Spi.Search.IntegrationTests.Context
{
    public class InMemoryLearningProviderSearchIndex : ILearningProviderSearchIndex
    {
        private LearningProviderSearchDocument[] _dataset;
        
        public Task CreateOrUpdateIndexAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }

        public Task UploadBatchAsync(LearningProviderSearchDocument[] documents, CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }

        public Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<LearningProviderSearchDocument> matches = _dataset;
            foreach (var filter in request.Filter)
            {
                if (filter.Field.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
                {
                    matches = matches.Where(d =>
                        d.Name.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                }
            }
            var results = new SearchResultset<LearningProviderSearchDocument>{Documents = matches.ToArray()};
            return Task.FromResult(results);
        }


        internal void SetIndexDataset(IEnumerable<LearningProviderSearchDocument> dataset)
        {
            _dataset = dataset?.ToArray() ?? new LearningProviderSearchDocument[0];
        }
    }
}