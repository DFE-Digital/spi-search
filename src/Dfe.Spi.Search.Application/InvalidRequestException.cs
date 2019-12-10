using System;
using System.Text;

namespace Dfe.Spi.Search.Application
{
    public class InvalidRequestException : Exception
    {
        public string[] Reasons { get; }

        public InvalidRequestException(params string[] reasons)
            : base(BuildDefaultMessage(reasons))
        {
            Reasons = reasons;
        }

        private static string BuildDefaultMessage(string[] reasons)
        {
            var message = new StringBuilder();

            message.Append("Request if not valid");
            foreach (var reason in reasons)
            {
                message.AppendLine($"  > {reason}");
            }

            return message.ToString();
        }
    }
}