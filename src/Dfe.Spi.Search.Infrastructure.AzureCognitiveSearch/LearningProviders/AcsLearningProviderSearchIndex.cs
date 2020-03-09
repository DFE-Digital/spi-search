using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.Configuration;
using Dfe.Spi.Search.Domain.LearningProviders;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.LearningProviders
{
    public class AcsLearningProviderSearchIndex : AcsSearchIndex<LearningProviderSearchDocument, AcsLearningProviderDocument>, ILearningProviderSearchIndex
    {
        public AcsLearningProviderSearchIndex(SearchIndexConfiguration configuration, ILoggerWrapper logger) 
            : base(configuration, configuration.LearningProviderIndexName, logger)
        {
        }

        protected override AcsLearningProviderDocument ConvertModelToSearchDocument(LearningProviderSearchDocument model)
        {
            return new AcsLearningProviderDocument
            {
                Id = $"{model.SourceSystemName}-{model.SourceSystemId}",
                Name = model.Name,
                Type = model.Type,
                SubType = model.SubType,
                Status = model.Status,
                OpenDate = model.OpenDate,
                CloseDate = model.CloseDate,
                Urn = model.Urn,
                Ukprn = model.Ukprn,
                Uprn = model.Uprn,
                CompaniesHouseNumber = model.CompaniesHouseNumber,
                CharitiesCommissionNumber = model.CharitiesCommissionNumber,
                AcademyTrustCode = model.AcademyTrustCode,
                DfeNumber = model.DfeNumber,
                LocalAuthorityCode = model.LocalAuthorityCode,
                ManagementGroupType = model.ManagementGroupType,
                ManagementGroupId = model.ManagementGroupId,
                SourceSystemName = model.SourceSystemName,
                SourceSystemId = model.SourceSystemId,
            };
        }

        protected override LearningProviderSearchDocument ConvertSearchToModelDocument(AcsLearningProviderDocument search)
        {
            return new LearningProviderSearchDocument
            {
                Name = search.Name,
                Type = search.Type,
                SubType = search.SubType,
                Status = search.Status,
                OpenDate = search.OpenDate,
                CloseDate = search.CloseDate,
                Urn = search.Urn,
                Ukprn = search.Ukprn,
                Uprn = search.Uprn,
                CompaniesHouseNumber = search.CompaniesHouseNumber,
                CharitiesCommissionNumber = search.CharitiesCommissionNumber,
                AcademyTrustCode = search.AcademyTrustCode,
                DfeNumber = search.DfeNumber,
                LocalAuthorityCode = search.LocalAuthorityCode,
                ManagementGroupType = search.ManagementGroupType,
                ManagementGroupId = search.ManagementGroupId,
                SourceSystemName = search.SourceSystemName,
                SourceSystemId = search.SourceSystemId,
            };
        }
    }
}