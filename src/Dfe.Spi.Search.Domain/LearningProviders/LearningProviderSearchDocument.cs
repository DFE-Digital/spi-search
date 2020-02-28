using System;
using Dfe.Spi.Search.Domain.Common;

namespace Dfe.Spi.Search.Domain.LearningProviders
{
    public class LearningProviderSearchDocument : SearchDocument
    {
        public virtual string Name { get; set; }
        public virtual string Type { get; set; }
        public virtual string SubType { get; set; }
        public virtual string Status { get; set; }
        public virtual DateTime? OpenDate { get; set; }
        public virtual DateTime? CloseDate { get; set; }
        public virtual long? Urn { get; set; }
        public virtual long? Ukprn { get; set; }
        public virtual string Uprn { get; set; }
        public virtual string CompaniesHouseNumber { get; set; }
        public virtual string CharitiesCommissionNumber { get; set; }
        public virtual string AcademyTrustCode { get; set; }
        public virtual string DfeNumber { get; set; }
        public virtual string LocalAuthorityCode { get; set; }
        public virtual long? EstablishmentNumber { get; set; }
        public virtual long? PreviousEstablishmentNumber { get; set; }
        public virtual string ManagementGroupType { get; set; }
        public virtual string ManagementGroupId { get; set; }
    }
}