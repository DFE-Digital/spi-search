using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.Configuration;
using Dfe.Spi.Search.Domain.LearningProviders;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.LearningProviders
{
    public class AcsLearningProviderSearchIndex : ILearningProviderSearchIndex
    {
        private readonly SearchIndexConfiguration _configuration;
        private readonly ILoggerWrapper _logger;

        public AcsLearningProviderSearchIndex(SearchIndexConfiguration configuration, ILoggerWrapper logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CreateOrUpdateIndexAsync(CancellationToken cancellationToken)
        {
            using (var client = GetManagementClient())
            {
                var definition = new Index()
                {
                    Name = _configuration.IndexName,
                    Fields = FieldBuilder.BuildForType<AcsLearningProviderDocument>()
                };

                await client.Indexes.CreateOrUpdateAsync(definition, cancellationToken: cancellationToken);
            }
        }

        public async Task UploadBatchAsync(LearningProviderSearchDocument[] documents,
            CancellationToken cancellationToken)
        {
            using (var client = GetIndexClient())
            {
                var batch = IndexBatch.Upload(documents.Select(d => new AcsLearningProviderDocument
                {
                    Id = $"{d.SourceSystemName}-{d.SourceSystemId}",
                    Name = d.Name,
                    SourceSystemName = d.SourceSystemName,
                    SourceSystemId = d.SourceSystemId,
                }));

                await client.Documents.IndexAsync(batch, cancellationToken: cancellationToken);
            }
        }

        public async Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request,
            CancellationToken cancellationToken)
        {
            var nameFilter = request.Filter.SingleOrDefault(f =>
                    f.Field.Equals("Name", StringComparison.InvariantCultureIgnoreCase));
            if (nameFilter == null)
            {
                throw new Exception("Can currently only query by name, which must be provided");
            }

            using (var client = GetIndexClient())
            {
                var query = $"Name: \"{nameFilter.Value}\"";
                _logger.Info($"Search ACS with query {query}...");

                var results = await client.Documents.SearchAsync<AcsLearningProviderDocument>(
                    query,
                    new SearchParameters { QueryType = QueryType.Full },
                    cancellationToken: cancellationToken);
                var documents = results.Results.Select(acs => new LearningProviderSearchDocument
                {
                    Name = acs.Document.Name,
                    SourceSystemName = acs.Document.SourceSystemName,
                    SourceSystemId = acs.Document.SourceSystemId,
                }).ToArray();
                return new SearchResultset<LearningProviderSearchDocument>
                {
                    Documents = documents,
                };
            }
        }

        private SearchServiceClient GetManagementClient()
        {
            return new SearchServiceClient(_configuration.AzureCognitiveSearchServiceName,
                new SearchCredentials(_configuration.AzureCognitiveSearchKey));
        }

        private SearchIndexClient GetIndexClient()
        {
            return new SearchIndexClient(_configuration.AzureCognitiveSearchServiceName, _configuration.IndexName,
                new SearchCredentials(_configuration.AzureCognitiveSearchKey));
        }
    }
}