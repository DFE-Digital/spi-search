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
                Code = model.Code,
                Type = model.Type,
                Identifier = model.Identifier,
                CompaniesHouseNumber = model.CompaniesHouseNumber,
                SourceSystemName = model.SourceSystemName,
                SourceSystemId = model.SourceSystemId,
            };
        }

        protected override ManagementGroupSearchDocument ConvertSearchToModelDocument(AcsManagementGroupSearchDocument search)
        {
            return new ManagementGroupSearchDocument
            {
                Name = search.Name,
                Code = search.Code,
                Type = search.Type,
                Identifier = search.Identifier,
                CompaniesHouseNumber = search.CompaniesHouseNumber,
                SourceSystemName = search.SourceSystemName,
                SourceSystemId = search.SourceSystemId,
            };
        }
    }
}