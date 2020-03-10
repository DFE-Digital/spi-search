using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.ManagementGroups;
using Newtonsoft.Json;

namespace Dfe.Spi.Search.Application.ManagementGroups
{
    public interface IManagementGroupSearchManager
    {
        Task<SearchResultset<ManagementGroupSearchDocument>> SearchAsync(SearchRequest request,
            CancellationToken cancellationToken);

        Task SyncAsync(ManagementGroup managementGroup, string source, CancellationToken cancellationToken);

        Task SyncBatchAsync(ManagementGroup[] managementGroups, string source, CancellationToken cancellationToken);
    }
    
    public class ManagementGroupSearchManager : IManagementGroupSearchManager
    {
        private readonly IManagementGroupSearchIndex _searchIndex;
        private readonly ILoggerWrapper _logger;

        public ManagementGroupSearchManager(
            IManagementGroupSearchIndex searchIndex,
            ILoggerWrapper logger)
        {
            _searchIndex = searchIndex;
            _logger = logger;
        }

        public async Task<SearchResultset<ManagementGroupSearchDocument>> SearchAsync(SearchRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureSearchRequestIsValid(request, cancellationToken);

            return await _searchIndex.SearchAsync(request, cancellationToken);
        }
        
        public async Task SyncAsync(ManagementGroup managementGroup, string source, CancellationToken cancellationToken)
        {
            var searchDocument = MapManagementGroupToSearchDocument(managementGroup, source);
            _logger.Info($"Mapped management group to search document: {JsonConvert.SerializeObject(searchDocument)}");
            
            await _searchIndex.UploadBatchAsync(new[] {searchDocument}, cancellationToken);
            _logger.Debug($"Successfully uploaded document to search index");
        }

        public async Task SyncBatchAsync(ManagementGroup[] managementGroups, string source, CancellationToken cancellationToken)
        {
            var searchDocuments =
                managementGroups.Select(x => MapManagementGroupToSearchDocument(x, source)).ToArray();
            _logger.Info(
                $"Mapped {managementGroups.Length} management groups to {searchDocuments.Length} search documents");

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
                
                default:
                    toReturn = StringOperators;
                    break;
            }

            return toReturn;
        }
        private ManagementGroupSearchDocument MapManagementGroupToSearchDocument(ManagementGroup managementGroup, string source)
        {
            return new ManagementGroupSearchDocument
            {
                Name = managementGroup.Name,
                Code = managementGroup.Code,
                Type = managementGroup.Type,
                Identifier = managementGroup.Identifier,
                CompaniesHouseNumber = managementGroup.CompaniesHouseNumber,
                SourceSystemName = source,
                SourceSystemId = managementGroup.Code,
            };
        }
        
        
        private static readonly string[] NonFilterableOperators = new[]
        {
            Operators.Contains,
        };

        private static readonly string[] StringOperators = new[]
        {
            Operators.Equals,
            Operators.IsNull,
            Operators.IsNotNull,
        };
    }
}