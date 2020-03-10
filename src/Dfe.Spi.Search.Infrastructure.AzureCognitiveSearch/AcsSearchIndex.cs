using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch
{
    public abstract class AcsSearchIndex<TModel, TSearch> : ISearchIndex<TModel> where TModel : SearchDocument
    {
        private readonly SearchIndexConfiguration _configuration;
        private readonly string _indexName;
        private readonly ILoggerWrapper _logger;
        private readonly SearchFieldDefinition[] _fieldDefinitions;

        protected AcsSearchIndex(SearchIndexConfiguration configuration, string indexName, ILoggerWrapper logger)
        {
            _configuration = configuration;
            _indexName = indexName;
            _logger = logger;
            
            var properties = typeof(TSearch).GetProperties();
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

            _fieldDefinitions = definitions.ToArray();
        }

        public virtual Task<string[]> GetSearchableFieldsAsync(CancellationToken cancellationToken)
        {
            var searchableFieldNames = _fieldDefinitions
                .Where(fd => fd.IsSearchable || fd.IsFilterable)
                .Select(fd => fd.Name)
                .ToArray();
            return Task.FromResult(searchableFieldNames);
        }

        public virtual async Task UploadBatchAsync(TModel[] documents, CancellationToken cancellationToken)
        {
            using (var client = GetIndexClient())
            {
                var batch = IndexBatch.Upload(documents.Select(ConvertModelToSearchDocument));

                await client.Documents.IndexAsync(batch, cancellationToken: cancellationToken);
            }
        }

        public virtual async Task<SearchResultset<TModel>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            using (var client = GetIndexClient())
            {
                var search = BuildSearch(request);
                _logger.Info($"Search ACS with query {search.Query} and filter {search.Filter}...");

                var results = await client.Documents.SearchAsync<TSearch>(
                    search.Query,
                    new SearchParameters
                    {
                        QueryType = QueryType.Full,
                        Filter = search.Filter,
                    },
                    cancellationToken: cancellationToken);

                var documents = results.Results.Select(acs => ConvertSearchToModelDocument(acs.Document)).ToArray();

                return new SearchResultset<TModel>
                {
                    Documents = documents,
                };
            }
        }

        protected abstract TSearch ConvertModelToSearchDocument(TModel model);

        protected abstract TModel ConvertSearchToModelDocument(TSearch search);

        protected virtual SearchIndexClient GetIndexClient()
        {
            return new SearchIndexClient(_configuration.AzureCognitiveSearchServiceName, _indexName,
                new SearchCredentials(_configuration.AzureCognitiveSearchKey));
        }
        
        protected virtual AcsSearch BuildSearch(SearchRequest request)
        {
            var search = new AcsSearch(request.CombinationOperator);

            foreach (var searchGroup in request.Groups)
            {
                var group = new AcsSearch(searchGroup.CombinationOperator);

                foreach (var requestFilter in searchGroup.Filter)
                {
                    var definition = _fieldDefinitions.Single(fd =>
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

        protected virtual string EncodeIdForAcs(string id)
        {
            var encodedId = new StringBuilder();
            
            foreach (var character in id)
            {
                if (char.IsLetterOrDigit(character) ||
                    character == '-' ||
                    character == '_' ||
                    character == '=')
                {
                    encodedId.Append(character);
                }
                else
                {
                    encodedId.Append('_');
                }
            }

            return encodedId.ToString();
        }
    }
}