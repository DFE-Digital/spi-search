namespace Dfe.Spi.Search.Domain.Common
{
    public abstract class SearchDocument
    {
        public virtual string SourceSystemName { get; set; }
        public virtual string SourceSystemId { get; set; }
    }
}