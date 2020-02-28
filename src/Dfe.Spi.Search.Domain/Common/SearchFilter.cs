namespace Dfe.Spi.Search.Domain.Common
{
    public class SearchFilter
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
    }

    public static class Operators
    {
        public const string Equals = "equals";
        public const string Contains = "contains";
        public const string GreaterThan = "greaterthan";
        public const string GreaterThanOrEqualTo = "greaterthanequalto";
        public const string LessThan = "lessthan";
        public const string LessThanOrEqualTo = "lessthanequalto";
        public const string In = "in";
        public const string IsNull = "isnull";
        public const string IsNotNull = "isnotnull";
    }
}