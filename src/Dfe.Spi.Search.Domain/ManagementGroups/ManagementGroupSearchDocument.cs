using Dfe.Spi.Search.Domain.Common;

namespace Dfe.Spi.Search.Domain.ManagementGroups
{
    public class ManagementGroupSearchDocument : SearchDocument
    {
        public virtual string Name { get; set; }
        public virtual string Type { get; set; }
        public virtual string Code { get; set; }
        public virtual string Identifier { get; set; }
        public virtual string CompaniesHouseNumber { get; set; }
    }
}