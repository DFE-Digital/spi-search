using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.LearningProviders;
using Dfe.Spi.Search.Functions;
using Dfe.Spi.Search.Functions.LearningProviders;
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

        public InMemoryLearningProviderSearchIndex GetInstanceOfLearningProviderSearchIndex()
        {
            return (InMemoryLearningProviderSearchIndex) GetInstance<ILearningProviderSearchIndex>();
        }

        public InProcLoggerWrapper GetInstanceOfLogger()
        {
            return (InProcLoggerWrapper) GetInstance<ILoggerWrapper>();
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
            services.AddScoped<ILoggerWrapper, InProcLoggerWrapper>();

            // Register functions
            services.AddScoped<SearchLearningProviders>();
        }
        
        
        private class HostBuilder : IFunctionsHostBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();
        }
    }
}