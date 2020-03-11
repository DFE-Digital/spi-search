namespace Dfe.Spi.Search.Domain.Common
{
    public class SearchResultset<T> where T : SearchDocument
    {
        public T[] Documents { get; set; }
        
        public int Skipped { get; set; }
        public int Taken { get; set; }
        public long TotalNumberOfDocuments { get; set; }
    }
}