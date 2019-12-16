using System;
using System.Collections.Generic;
using Dfe.Spi.Common.Logging.Definitions;
using Microsoft.AspNetCore.Http;

namespace Dfe.Spi.Search.IntegrationTests.Context
{
    public class InProcLoggerWrapper : ILoggerWrapper
    {
        private List<string> _logMessages = new List<string>();
        
        public void SetContext(IHeaderDictionary headerDictionary)
        {
        }

        public void SetInternalRequestId(Guid internalRequestId)
        {
        }

        public void Debug(string message)
        {
            _logMessages.Add($"DEBUG: {message}");
        }

        public void Info(string message)
        {
            _logMessages.Add($"INFO: {message}");
        }

        public void Warning(string message)
        {
            _logMessages.Add($"WARN: {message}");
        }

        public void Warning(string message, Exception exception)
        {
            _logMessages.Add($"WARN: {message} {exception}");
        }

        public void Error(string message)
        {
            _logMessages.Add($"ERROR: {message}");
        }

        public void Error(string message, Exception exception)
        {
            _logMessages.Add($"ERROR: {message} {exception}");
        }

        public string[] GetLogs()
        {
            return _logMessages.ToArray();
        }
    }
}