using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.LearningProviders;
using Dfe.Spi.Search.Functions;
using Dfe.Spi.Search.Functions.LearningProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Dfe.Spi.Search.IntegrationTests.Context
{
    public class TestFunctionHost
    {
        private ServiceProvider _provider;

        public TestFunctionHost()
        {
            var services = GetDefaultServiceCollection();
            UpdateTestServices(services);
            _provider = services.BuildServiceProvider();
        }

        public T GetInstance<T>()
        {
            return _provider.GetService<T>();
        }

        private IServiceCollection GetDefaultServiceCollection()
        {
            var builder = new HostBuilder();
            var startup = new Startup();
            startup.Configure(builder);
            return builder.Services;
        }

        private void UpdateTestServices(IServiceCollection services)
        {
            // Change infrastructure references for test implementations
            services.AddScoped<ILearningProviderSearchIndex, InMemoryLearningProviderSearchIndex>();
            services.AddScoped<ILoggerWrapper, ConsoleLoggerWrapper>();

            // Register functions
            services.AddScoped<SearchLearningProviders>();
        }
        
        
        private class HostBuilder : IFunctionsHostBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();
        }
    }

    public class InMemoryLearningProviderSearchIndex : ILearningProviderSearchIndex
    {
        public Task CreateOrUpdateIndexAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }

        public Task UploadBatchAsync(LearningProviderSearchDocument[] documents, CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }

        public Task<SearchResultset<LearningProviderSearchDocument>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            var results = new SearchResultset<LearningProviderSearchDocument>{Documents = new LearningProviderSearchDocument[0]};
            return Task.FromResult(results);
        }
    }

    public class ConsoleLoggerWrapper : ILoggerWrapper
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