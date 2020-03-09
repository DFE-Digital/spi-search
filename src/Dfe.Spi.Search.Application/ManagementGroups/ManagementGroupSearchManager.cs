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
        // Task<SearchResultset<ManagementGroupSearchDocument>> SearchAsync(SearchRequest request,
        //     CancellationToken cancellationToken);

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
    }
}