namespace Dfe.Spi.Search.Domain.Common
{
    public class SearchRequest
    {
        public SearchGroup[] Groups { get; set; }
        public string CombinationOperator { get; set; }
    }
}