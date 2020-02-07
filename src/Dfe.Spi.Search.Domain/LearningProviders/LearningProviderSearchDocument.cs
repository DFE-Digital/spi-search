using Dfe.Spi.Search.Domain.Common;

namespace Dfe.Spi.Search.Domain.LearningProviders
{
    public class LearningProviderSearchDocument : SearchDocument
    {
        public virtual string Name { get; set; }
        public virtual long? Urn { get; set; }
        public virtual long? Ukprn { get; set; }
        public virtual string Uprn { get; set; }
        public virtual string CompaniesHouseNumber { get; set; }
        public virtual string CharitiesCommissionNumber { get; set; }
        public virtual string AcademyTrustCode { get; set; }
        public virtual string DfeNumber { get; set; }
        public virtual long? EstablishmentNumber { get; set; }
        public virtual long? PreviousEstablishmentNumber { get; set; }
    }
}