using Dfe.Spi.Search.Domain.Common;

namespace Dfe.Spi.Search.Domain.LearningProviders
{
    public class LearningProviderSearchDocument : SearchDocument
    {
        public virtual string Name { get; set; }
    }
}