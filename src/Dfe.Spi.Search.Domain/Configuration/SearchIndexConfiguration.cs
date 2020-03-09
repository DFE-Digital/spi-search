namespace Dfe.Spi.Search.Domain.Configuration
{
    public class SearchIndexConfiguration
    {
        public string LearningProviderIndexName { get; set; }
        public string ManagementGroupIndexName { get; set; }
        
        public string AzureCognitiveSearchServiceName { get; set; }
        public string AzureCognitiveSearchKey { get; set; }
    }
}