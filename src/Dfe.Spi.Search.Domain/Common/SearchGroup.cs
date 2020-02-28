namespace Dfe.Spi.Search.Domain.Common
{
    public class SearchGroup
    {
        public SearchFilter[] Filter { get; set; }
        public string CombinationOperator { get; set; }
    }
}