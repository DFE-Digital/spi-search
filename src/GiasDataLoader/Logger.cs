using System;
using Dfe.Spi.Common.Logging.Definitions;

namespace GiasDataLoader
{
    class Logger : ILoggerWrapper
    {
        public void SetContext(Microsoft.AspNetCore.Http.IHeaderDictionary headerDictionary)
        {
        }

        public void SetInternalRequestId(Guid internalRequestId)
        {
        }

        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void Warning(string message)
        {
            WriteColored(message, ConsoleColor.Yellow);
        }

        public void Warning(string message, Exception exception)
        {
            WriteColored(message + "\n" + exception.ToString(), ConsoleColor.Yellow);
        }
        
        public void Error(string message)
        {
            WriteColored(message, ConsoleColor.Red);
        }

        public void Error(string message, Exception exception)
        {
            WriteColored(message + "\n" + exception.ToString(), ConsoleColor.Yellow);
        }

        public void Error(Exception exception)
        {
            WriteColored(exception.ToString(), ConsoleColor.Red);
        }

        private void WriteColored(string message, ConsoleColor color)
        {
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}