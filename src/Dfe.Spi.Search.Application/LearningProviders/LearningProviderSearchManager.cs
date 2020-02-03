using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.WellKnownIdentifiers;
using Dfe.Spi.Models;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.LearningProviders;
using Newtonsoft.Json;

namespace Dfe.Spi.Search.Application.LearningProviders
{
    public interface ILearningProviderSearchManager
    {
        Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request,
            CancellationToken cancellationToken);

        Task SyncAsync(LearningProvider learningProvider, string source, CancellationToken cancellationToken);
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

        public async Task SyncAsync(LearningProvider learningProvider, string source, CancellationToken cancellationToken)
        {
            var searchDocument = MapLearningProviderToSearchDocument(learningProvider, source);
            _logger.Info($"Mapped learning provider to search document: {JsonConvert.SerializeObject(searchDocument)}");

            await _searchIndex.UploadBatchAsync(new[] {searchDocument}, cancellationToken);
            _logger.Debug($"Successfully uploaded document to search index");
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

        private LearningProviderSearchDocument MapLearningProviderToSearchDocument(LearningProvider learningProvider, string source)
        {
            var searchDocument = new LearningProviderSearchDocument
            {
                SourceSystemName = source,
                Name = learningProvider.Name,
            };

            if (source == SourceSystemNames.UkRegisterOfLearningProviders)
            {
                searchDocument.SourceSystemId = learningProvider.Ukprn.ToString();
            }
            else
            {
                searchDocument.SourceSystemId = learningProvider.Urn.ToString();
            }
            
            return searchDocument;
        }
    }
}