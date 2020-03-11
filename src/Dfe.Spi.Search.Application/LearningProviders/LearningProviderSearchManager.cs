using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.WellKnownIdentifiers;
using Dfe.Spi.Models.Entities;
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
            if (request.Take == 0)
            {
                request.Take = 50;
            }
            
            await EnsureSearchRequestIsValid(request, cancellationToken);

            return await _searchIndex.SearchAsync(request, cancellationToken);
        }

        public async Task SyncAsync(LearningProvider learningProvider, string source,
            CancellationToken cancellationToken)
        {
            var searchDocument = MapLearningProviderToSearchDocument(learningProvider, source);
            _logger.Info($"Mapped learning provider to search document: {JsonConvert.SerializeObject(searchDocument)}");

            await _searchIndex.UploadBatchAsync(new[] {searchDocument}, cancellationToken);
            _logger.Debug($"Successfully uploaded document to search index");
        }

        public async Task SyncBatchAsync(LearningProvider[] learningProviders, string source,
            CancellationToken cancellationToken)
        {
            var searchDocuments =
                learningProviders.Select(x => MapLearningProviderToSearchDocument(x, source)).ToArray();
            _logger.Info(
                $"Mapped {learningProviders.Length} learning providers to {searchDocuments.Length} search documents");

            await _searchIndex.UploadBatchAsync(searchDocuments, cancellationToken);
            _logger.Debug($"Successfully uploaded document to search index");
        }


        private async Task EnsureSearchRequestIsValid(SearchRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new InvalidRequestException("Must provide SearchRequest");
            }

            if (request.Groups == null)
            {
                throw new InvalidRequestException("Must provide groups");
            }

            if (request.CombinationOperator != "and" && request.CombinationOperator != "or")
            {
                throw new InvalidRequestException("Request combinationOperator must be either 'and' or 'or'");
            }


            var validationProblems = new List<string>();
            var searchableFields = await _searchIndex.GetSearchableFieldsAsync(cancellationToken);
            for (var i = 0; i < request.Groups.Length; i++)
            {
                if (request.Groups[i].CombinationOperator != "and" && request.Groups[i].CombinationOperator != "or")
                {
                    validationProblems.Add($"Group {i} combinationOperator must be either 'and' or 'or'");
                }
                if (request.Groups[i].Filter == null)
                {
                    validationProblems.Add($"Group {i} must have filters");
                    continue;
                }
                
                foreach (var filter in request.Groups[i].Filter)
                {
                    filter.Operator = filter.Operator ?? GetDefaultOperatorForField(filter.Field);

                    if (!searchableFields.Any(f => f.Equals(filter.Field, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        validationProblems.Add($"{filter.Field} in group {i} is not a valid field for filtering");
                    }

                    var validOperators = GetValidOperatorsForField(filter.Field);
                    if (!validOperators.Any(o =>
                        o.Equals(filter.Operator, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        validationProblems.Add($"Operator {filter.Operator} is not valid for {filter.Field} in group {i}");
                    }
                }
            }

            if (validationProblems.Count > 0)
            {
                throw new InvalidRequestException(validationProblems.ToArray());
            }
        }

        private string GetDefaultOperatorForField(string field)
        {
            switch (field.ToLower())
            {
                case "name":
                    return Operators.Contains;
                default:
                    return Operators.Equals;
            }
        }

        private string[] GetValidOperatorsForField(string field)
        {
            string[] toReturn = null;

            string fieldLower = field.ToLower();

            switch (fieldLower)
            {
                case "name":
                    toReturn = NonFilterableOperators;
                    break;

                case "type":
                case "subtype":
                case "status":
                case "managementgrouptype":
                    toReturn = EnumOperators;
                    break;

                case "opendate":
                case "closedate":
                    toReturn = DateOperators;
                    break;

                default:
                    toReturn = StringOperators;
                    break;
            }

            return toReturn;
        }

        private LearningProviderSearchDocument MapLearningProviderToSearchDocument(LearningProvider learningProvider,
            string source)
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
                LocalAuthorityCode = learningProvider.LocalAuthorityCode,
                ManagementGroupType = learningProvider.ManagementGroup?.Type,
                ManagementGroupId = learningProvider.ManagementGroup?.Identifier,
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


        private static readonly string[] NonFilterableOperators = new[]
        {
            Operators.Contains,
        };

        private static readonly string[] EnumOperators = new[]
        {
            Operators.Equals,
            Operators.In,
            Operators.IsNull,
            Operators.IsNotNull,
        };

        private static readonly string[] DateOperators = new[]
        {
            Operators.Equals,
            Operators.GreaterThan,
            Operators.GreaterThanOrEqualTo,
            Operators.LessThan,
            Operators.LessThanOrEqualTo,
            Operators.IsNull,
            Operators.IsNotNull,
            Operators.Between,
        };

        private static readonly string[] StringOperators = new[]
        {
            Operators.Equals,
            Operators.IsNull,
            Operators.IsNotNull,
        };
    }
}