using System;
using System.Collections.Generic;
using System.Linq;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.LearningProviders;

namespace Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch
{
    public class AcsSearch
    {
        public AcsSearch(string combinationOperator)
        {
            CombinationOperator = combinationOperator;
        }

        public string CombinationOperator { get; }
        public string Query { get; set; } = "";
        public string Filter { get; set; }

        
        public void AppendQuery(SearchFieldDefinition field, string value)
        {
            if (IsNumericType(field.DataType))
            {
                AppendQuery($"{field.Name}: {value})");
            }
            else
            {
                AppendQuery($"{field.Name}: \"{value}\"");
            }
        }

        public void AppendQuery(string value)
        {
            if (Query?.Length > 0)
            {
                Query += $" {CombinationOperator} {value}";
            }
            else
            {
                Query = value;
            }
        }

        public void AppendFilter(SearchFieldDefinition field, string filterOperator, string value)
        {
            if (field.IsSearchable)
            {
                AppendFilter($"search.ismatch('\"{value}\"', '{field.Name}')");
                return;
            }
            
            if (filterOperator == Operators.Between)
            {
                string[] dateParts = value.Split(
                    new string[] { " to " },
                    StringSplitOptions.RemoveEmptyEntries);

                if (dateParts.Length != 2)
                {
                    // Then get upset 💢
                    throw new FormatException(
                        $"Between values need to contain 2 valid " +
                        $"{nameof(DateTime)}s, seperated by the keyword " +
                        $"\"to\". For example, \"2018-06-29T00:00:00Z\" to " +
                        $"\"2018-07-01T00:00:00Z\".");
                }

                // Else...
                // Try and build up a group query of our own.
                AcsSearch between = new AcsSearch("and");
                between.AppendFilter(field, Operators.LessThan, dateParts.Last());
                between.AppendFilter(field, Operators.GreaterThan, dateParts.First());

                AddGroup(between);
            }
            else {

                if (filterOperator == Operators.In)
                {
                    var values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                    var conditionValue = values.Aggregate((x, y) => $"{x},{y}");
                    AppendFilter($"search.in({field.Name}, '{conditionValue}', ',')");
                }
                else
                {
                    string conditionValue;
                    if (filterOperator == Operators.IsNull || filterOperator == Operators.IsNotNull)
                    {
                        conditionValue = "null";
                    }
                    else
                    {
                        if (IsNumericType(field.DataType))
                        {
                            conditionValue = value;
                        }
                        else if (IsDateType(field.DataType))
                        {
                            DateTime dtm;
                            if (!DateTime.TryParse(value, out dtm))
                            {
                                throw new Exception($"{value} is not a valid date/time value for {field.Name}");
                            }

                            conditionValue = dtm.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                        else
                        {
                            conditionValue = $"'{value}'";
                        }
                    }

                    var acsOperator = OperatorMappings[filterOperator.ToLower()];
                    AppendFilter($"{field.Name} {acsOperator} {conditionValue}");
                }
            }
        }

        public void AppendFilter(string value)
        {
            if (Filter?.Length > 0)
            {
                Filter += $" {CombinationOperator} {value}";
            }
            else
            {
                Filter = value;
            }
        }

        public void AddGroup(AcsSearch group)
        {
            if (!string.IsNullOrEmpty(group.Query) && group.Query != "*")
            {
                AppendQuery($"({group.Query})");
            }
            if (!string.IsNullOrEmpty(group.Filter))
            {
                AppendFilter($"({group.Filter})");
            }
        }
        
        

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) ||
                   type == typeof(int?) ||
                   type == typeof(long) ||
                   type == typeof(long?);
        }

        private bool IsDateType(Type type)
        {
            return type == typeof(DateTime) ||
                   type == typeof(DateTime?);
        }

        private static readonly Dictionary<string, string> OperatorMappings = new Dictionary<string, string>
        {
            {Operators.Equals.ToLower(), "eq"},
            {Operators.GreaterThan.ToLower(), "gt"},
            {Operators.GreaterThanOrEqualTo.ToLower(), "ge"},
            {Operators.LessThan.ToLower(), "lt"},
            {Operators.LessThanOrEqualTo.ToLower(), "le"},
            {Operators.IsNull.ToLower(), "eq"},
            {Operators.IsNotNull.ToLower(), "ne"}
        };
    }
}