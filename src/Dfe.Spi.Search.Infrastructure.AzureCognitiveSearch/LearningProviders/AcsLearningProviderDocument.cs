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
    }
}