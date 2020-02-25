using System;
using Dfe.Spi.Search.Domain.LearningProviders;
using Microsoft.Azure.Search;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.LearningProviders
{
    public class AcsLearningProviderDocument : LearningProviderSearchDocument
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string Id { get; set; }
        
        
        [IsSearchable, IsSortable]
        public override string Name { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override string Type { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override string SubType { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override string Status { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override DateTime? OpenDate { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override DateTime? CloseDate { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override long? Urn { get; set; }

        [IsFilterable, IsSortable] 
        public override long? Ukprn { get; set; }

        [IsFilterable, IsSortable] 
        public override string Uprn { get; set; }

        [IsFilterable, IsSortable] 
        public override string CompaniesHouseNumber { get; set; }

        [IsFilterable, IsSortable] 
        public override string CharitiesCommissionNumber { get; set; }

        [IsFilterable, IsSortable] 
        public override string AcademyTrustCode { get; set; }

        [IsFilterable, IsSortable] 
        public override string DfeNumber { get; set; }

        [IsFilterable, IsSortable] 
        public override long? EstablishmentNumber { get; set; }

        [IsFilterable, IsSortable] 
        public override long? PreviousEstablishmentNumber { get; set; }

        [IsFilterable, IsSortable] 
        public override string ManagementGroupType { get; set; }

        [IsFilterable, IsSortable] 
        public override string ManagementGroupId { get; set; }
    }
}