namespace Dfe.Spi.Search.Domain.Common
{
    public class SearchResultset<T> where T : SearchDocument
    {
        public T[] Documents { get; set; }
    }
}