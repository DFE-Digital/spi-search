using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.Configuration;
using Dfe.Spi.Search.Domain.ManagementGroups;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.ManagementGroups
{
    public class AcsManagementGroupSearchIndex : AcsSearchIndex<ManagementGroupSearchDocument, AcsManagementGroupSearchDocument>, IManagementGroupSearchIndex
    {
        public AcsManagementGroupSearchIndex(SearchIndexConfiguration configuration, ILoggerWrapper logger) 
            : base(configuration, configuration.ManagementGroupIndexName, logger)
        {
        }

        protected override AcsManagementGroupSearchDocument ConvertModelToSearchDocument(ManagementGroupSearchDocument model)
        {
            return new AcsManagementGroupSearchDocument
            {
                Id = EncodeIdForAcs($"{model.SourceSystemName}-{model.SourceSystemId}"),
                Name = model.Name,
                Code = model.Name,
                Type = model.Name,
                Identifier = model.Name,
                CompaniesHouseNumber = model.Name,
            };
        }

        protected override ManagementGroupSearchDocument ConvertSearchToModelDocument(AcsManagementGroupSearchDocument search)
        {
            return new ManagementGroupSearchDocument
            {
                Name = search.Name,
                Code = search.Name,
                Type = search.Name,
                Identifier = search.Name,
                CompaniesHouseNumber = search.Name,
            };
        }
    }
}