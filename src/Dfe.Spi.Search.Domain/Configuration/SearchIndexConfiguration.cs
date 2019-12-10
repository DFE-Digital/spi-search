namespace Dfe.Spi.Search.Domain.Configuration
{
    public class SearchIndexConfiguration
    {
        public string IndexName { get; set; }
        
        public string AzureCognitiveSearchServiceName { get; set; }
        public string AzureCognitiveSearchKey { get; set; }
    }
}