using Dfe.Spi.Search.Domain.ManagementGroups;
using Microsoft.Azure.Search;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.ManagementGroups
{
    public class AcsManagementGroupSearchDocument : ManagementGroupSearchDocument
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string Id { get; set; }
        
        
        [IsSearchable, IsFilterable, IsSortable]
        public override string Name { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override string Type { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override string Code { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override string Identifier { get; set; }
        
        
        [IsFilterable, IsSortable]
        public override string CompaniesHouseNumber { get; set; }
    }
}