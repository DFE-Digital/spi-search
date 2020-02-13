using System;
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

        Task SyncBatchAsync(LearningProvider[] learningProvider, string source, CancellationToken cancellationToken);
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
            await EnsureSearchRequestIsValid(request, cancellationToken);

            return await _searchIndex.SearchAsync(request, cancellationToken);
        }

        public async Task SyncAsync(LearningProvider learningProvider, string source, CancellationToken cancellationToken)
        {
            var searchDocument = MapLearningProviderToSearchDocument(learningProvider, source);
            _logger.Info($"Mapped learning provider to search document: {JsonConvert.SerializeObject(searchDocument)}");

            await _searchIndex.UploadBatchAsync(new[] {searchDocument}, cancellationToken);
            _logger.Debug($"Successfully uploaded document to search index");
        }

        public async Task SyncBatchAsync(LearningProvider[] learningProviders, string source, CancellationToken cancellationToken)
        {
            var searchDocuments =
                learningProviders.Select(x => MapLearningProviderToSearchDocument(x, source)).ToArray();
            _logger.Info($"Mapped {learningProviders.Length} learning providers to {searchDocuments.Length} search documents");

            await _searchIndex.UploadBatchAsync(searchDocuments, cancellationToken);
            _logger.Debug($"Successfully uploaded document to search index");
        }


        private async Task EnsureSearchRequestIsValid(SearchRequest request, CancellationToken cancellationToken)
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
            var searchableFields = await _searchIndex.GetSearchableFieldsAsync(cancellationToken);
            foreach (var filter in request.Filter)
            {
                if (!searchableFields.Any(f => f.Equals(filter.Field, StringComparison.InvariantCultureIgnoreCase)))
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
                Type = learningProvider.Type,
                SubType = learningProvider.SubType,
                Status = learningProvider.Status,
                OpenDate = learningProvider.OpenDate,
                CloseDate = learningProvider.CloseDate,
                Urn = learningProvider.Urn,
                Ukprn = learningProvider.Ukprn,
                Uprn = learningProvider.Uprn,
                CompaniesHouseNumber = learningProvider.CompaniesHouseNumber,
                CharitiesCommissionNumber = learningProvider.CharitiesCommissionNumber,
                AcademyTrustCode = learningProvider.AcademyTrustCode,
                DfeNumber = learningProvider.DfeNumber,
                EstablishmentNumber = learningProvider.EstablishmentNumber,
                PreviousEstablishmentNumber = learningProvider.PreviousEstablishmentNumber,
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