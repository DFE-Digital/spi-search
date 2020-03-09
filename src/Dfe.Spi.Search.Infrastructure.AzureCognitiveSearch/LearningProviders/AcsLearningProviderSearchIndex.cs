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
                    Name = _configuration.LearningProviderIndexName,
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
                    Type = d.Type,
                    SubType = d.SubType,
                    Status = d.Status,
                    OpenDate = d.OpenDate,
                    CloseDate = d.CloseDate,
                    Urn = d.Urn,
                    Ukprn = d.Ukprn,
                    Uprn = d.Uprn,
                    CompaniesHouseNumber = d.CompaniesHouseNumber,
                    CharitiesCommissionNumber = d.CharitiesCommissionNumber,
                    AcademyTrustCode = d.AcademyTrustCode,
                    DfeNumber = d.DfeNumber,
                    LocalAuthorityCode = d.LocalAuthorityCode,
                    ManagementGroupType = d.ManagementGroupType,
                    ManagementGroupId = d.ManagementGroupId,
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
                    Type = acs.Document.Type,
                    SubType = acs.Document.SubType,
                    Status = acs.Document.Status,
                    OpenDate = acs.Document.OpenDate,
                    CloseDate = acs.Document.CloseDate,
                    Urn = acs.Document.Urn,
                    Ukprn = acs.Document.Ukprn,
                    Uprn = acs.Document.Uprn,
                    CompaniesHouseNumber = acs.Document.CompaniesHouseNumber,
                    CharitiesCommissionNumber = acs.Document.CharitiesCommissionNumber,
                    AcademyTrustCode = acs.Document.AcademyTrustCode,
                    DfeNumber = acs.Document.DfeNumber,
                    LocalAuthorityCode = acs.Document.LocalAuthorityCode,
                    ManagementGroupType = acs.Document.ManagementGroupType,
                    ManagementGroupId = acs.Document.ManagementGroupId,
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
            var search = new AcsSearch(request.CombinationOperator);

            foreach (var searchGroup in request.Groups)
            {
                var group = new AcsSearch(searchGroup.CombinationOperator);

                foreach (var requestFilter in searchGroup.Filter)
                {
                    var definition = FieldDefinitions.Single(fd =>
                        fd.Name.Equals(requestFilter.Field, StringComparison.InvariantCultureIgnoreCase));

                    if (definition.IsSearchable || definition.IsFilterable)
                    {
                        group.AppendFilter(definition, requestFilter.Operator, requestFilter.Value);
                    }
                    else
                    {
                        throw new Exception($"{requestFilter.Field} is neither searchable nor filterable");
                    }
                }

                search.AddGroup(group);
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
            return new SearchIndexClient(_configuration.AzureCognitiveSearchServiceName, _configuration.LearningProviderIndexName,
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
        public AcsSearch(string combinationOperator)
        {
            CombinationOperator = combinationOperator;
        }

        public string CombinationOperator { get; }
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
                Query += $" {CombinationOperator} {value}";
            }
            else
            {
                Query = value;
            }
        }

        public void AppendFilter(SearchFieldDefinition field, string filterOperator, string value)
        {
            if (field.IsSearchable)
            {
                AppendFilter($"search.ismatch('\"{value}\"', '{field.Name}')");
                return;
            }
            
            if (filterOperator == Operators.Between)
            {
                string[] dateParts = value.Split(
                    new string[] { " to " },
                    StringSplitOptions.RemoveEmptyEntries);

                if (dateParts.Length != 2)
                {
                    // Then get upset 💢
                    throw new FormatException(
                        $"Between values need to contain 2 valid " +
                        $"{nameof(DateTime)}s, seperated by the keyword " +
                        $"\"to\". For example, \"2018-06-29T00:00:00Z\" to " +
                        $"\"2018-07-01T00:00:00Z\".");
                }

                // Else...
                // Try and build up a group query of our own.
                AcsSearch between = new AcsSearch("and");
                between.AppendFilter(field, Operators.LessThan, dateParts.Last());
                between.AppendFilter(field, Operators.GreaterThan, dateParts.First());

                AddGroup(between);
            }
            else {

                if (filterOperator == Operators.In)
                {
                    var values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                    var conditionValue = values.Aggregate((x, y) => $"{x},{y}");
                    AppendFilter($"search.in({field.Name}, '{conditionValue}', ',')");
                }
                else
                {
                    string conditionValue;
                    if (filterOperator == Operators.IsNull || filterOperator == Operators.IsNotNull)
                    {
                        conditionValue = "null";
                    }
                    else
                    {
                        if (IsNumericType(field.DataType))
                        {
                            conditionValue = value;
                        }
                        else if (IsDateType(field.DataType))
                        {
                            DateTime dtm;
                            if (!DateTime.TryParse(value, out dtm))
                            {
                                throw new Exception($"{value} is not a valid date/time value for {field.Name}");
                            }

                            conditionValue = dtm.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                        else
                        {
                            conditionValue = $"'{value}'";
                        }
                    }

                    var acsOperator = OperatorMappings[filterOperator.ToLower()];
                    AppendFilter($"{field.Name} {acsOperator} {conditionValue}");
                }
            }
        }

        public void AppendFilter(string value)
        {
            if (Filter?.Length > 0)
            {
                Filter += $" {CombinationOperator} {value}";
            }
            else
            {
                Filter = value;
            }
        }

        public void AddGroup(AcsSearch group)
        {
            if (!string.IsNullOrEmpty(group.Query) && group.Query != "*")
            {
                AppendQuery($"({group.Query})");
            }
            if (!string.IsNullOrEmpty(group.Filter))
            {
                AppendFilter($"({group.Filter})");
            }
        }
        
        

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) ||
                   type == typeof(int?) ||
                   type == typeof(long) ||
                   type == typeof(long?);
        }

        private bool IsDateType(Type type)
        {
            return type == typeof(DateTime) ||
                   type == typeof(DateTime?);
        }

        private static readonly Dictionary<string, string> OperatorMappings = new Dictionary<string, string>
        {
            {Operators.Equals.ToLower(), "eq"},
            {Operators.GreaterThan.ToLower(), "gt"},
            {Operators.GreaterThanOrEqualTo.ToLower(), "ge"},
            {Operators.LessThan.ToLower(), "lt"},
            {Operators.LessThanOrEqualTo.ToLower(), "le"},
            {Operators.IsNull.ToLower(), "eq"},
            {Operators.IsNotNull.ToLower(), "ne"}
        };
    }
}