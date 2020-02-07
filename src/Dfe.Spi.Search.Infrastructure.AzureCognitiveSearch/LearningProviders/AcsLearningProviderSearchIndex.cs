using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static readonly SearchFieldDefinition[] FieldDefinitions;
        
        private readonly SearchIndexConfiguration _configuration;
        private readonly ILoggerWrapper _logger;

        static AcsLearningProviderSearchIndex()
        {
            var properties = typeof(AcsLearningProviderDocument).GetProperties();
            var definitions = new List<SearchFieldDefinition>();
            
            foreach (var property in properties)
            {
                definitions.Add(new SearchFieldDefinition
                {
                    Name = property.Name,
                    DataType = property.PropertyType,
                    IsSearchable = property.GetCustomAttribute(typeof(IsSearchableAttribute)) != null,
                    IsFilterable = property.GetCustomAttribute(typeof(IsFilterableAttribute)) != null,
                });
            }

            FieldDefinitions = definitions.ToArray();
        }

        public AcsLearningProviderSearchIndex(SearchIndexConfiguration configuration, ILoggerWrapper logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<string[]> GetSearchableFieldsAsync(CancellationToken cancellationToken)
        {
            var searchableFieldNames = FieldDefinitions
                .Where(fd => fd.IsSearchable || fd.IsFilterable)
                .Select(fd => fd.Name)
                .ToArray();
            return Task.FromResult(searchableFieldNames);
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
                    Urn = d.Urn,
                    Ukprn = d.Ukprn,
                    Uprn = d.Uprn,
                    CompaniesHouseNumber = d.CompaniesHouseNumber,
                    CharitiesCommissionNumber = d.CharitiesCommissionNumber,
                    AcademyTrustCode = d.AcademyTrustCode,
                    DfeNumber = d.DfeNumber,
                    EstablishmentNumber = d.EstablishmentNumber,
                    PreviousEstablishmentNumber = d.PreviousEstablishmentNumber,
                    SourceSystemName = d.SourceSystemName,
                    SourceSystemId = d.SourceSystemId,
                }));

                await client.Documents.IndexAsync(batch, cancellationToken: cancellationToken);
            }
        }

        public async Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request,
            CancellationToken cancellationToken)
        {
            using (var client = GetIndexClient())
            {
                var search = BuildSearch(request);
                _logger.Info($"Search ACS with query {search.Query} and filter {search.Filter}...");
                
                var results = await client.Documents.SearchAsync<AcsLearningProviderDocument>(
                    search.Query,
                    new SearchParameters
                    {
                        QueryType = QueryType.Full,
                        Filter = search.Filter,
                    },
                    cancellationToken: cancellationToken);
                var documents = results.Results.Select(acs => new LearningProviderSearchDocument
                {
                    Name = acs.Document.Name,
                    Urn = acs.Document.Urn,
                    Ukprn = acs.Document.Ukprn,
                    Uprn = acs.Document.Uprn,
                    CompaniesHouseNumber = acs.Document.CompaniesHouseNumber,
                    CharitiesCommissionNumber = acs.Document.CharitiesCommissionNumber,
                    AcademyTrustCode = acs.Document.AcademyTrustCode,
                    DfeNumber = acs.Document.DfeNumber,
                    EstablishmentNumber = acs.Document.EstablishmentNumber,
                    PreviousEstablishmentNumber = acs.Document.PreviousEstablishmentNumber,
                    SourceSystemName = acs.Document.SourceSystemName,
                    SourceSystemId = acs.Document.SourceSystemId,
                }).ToArray();
                return new SearchResultset<LearningProviderSearchDocument>
                {
                    Documents = documents,
                };
            }
        }



        private AcsSearch BuildSearch(SearchRequest request)
        {
            var search = new AcsSearch();

            foreach (var requestFilter in request.Filter)
            {
                var definition = FieldDefinitions.Single(fd =>
                    fd.Name.Equals(requestFilter.Field, StringComparison.InvariantCultureIgnoreCase));

                if (definition.IsSearchable)
                {
                    search.AppendQuery(definition, requestFilter.Value);
                }
                else if (definition.IsFilterable)
                {
                    search.AppendFilter(definition, requestFilter.Value);
                }
                else
                {
                    throw new Exception($"{requestFilter.Field} is neither searchable nor filterable");
                }
            }

            if (string.IsNullOrEmpty(search.Query))
            {
                search.Query = "*";
            }

            return search;
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

    internal class SearchFieldDefinition
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsFilterable { get; set; }

        public override string ToString()
        {
            return $"{DataType} {Name} (searchable: {IsSearchable}, filterable: {IsFilterable})";
        }
    }

    internal class AcsSearch
    {
        public string Query { get; set; } = "";
        public string Filter { get; set; }

        public void AppendQuery(SearchFieldDefinition field, string value)
        {
            if (IsNumericType(field.DataType))
            {
                AppendQuery($"{field.Name}: {value})");
            }
            else
            {
                AppendQuery($"{field.Name}: \"{value}\"");
            }
        }
        public void AppendQuery(string value)
        {
            if (Query?.Length > 0)
            {
                Query += $" and {value}";
            }
            else
            {
                Query = value;
            }
        }

        public void AppendFilter(SearchFieldDefinition field, string value)
        {
            if (IsNumericType(field.DataType))
            {
                AppendFilter($"{field.Name} eq {value}");
            }
            else
            {
                AppendFilter($"{field.Name} eq '{value}'");
            }
        }
        public void AppendFilter(string value)
        {
            if (Filter?.Length > 0)
            {
                Filter += $" and {value}";
            }
            else
            {
                Filter = value;
            }
        }

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) ||
                   type == typeof(int?) ||
                   type == typeof(long) ||
                   type == typeof(long?);
        }
    }
}