using System;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch
{
    public class SearchFieldDefinition
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsFilterable { get; set; }

        public override string ToString()
        {
            return $"{DataType} {Name} (searchable: {IsSearchable}, filterable: {IsFilterable})";
        }
    }
}