using System.IO;
using Dfe.Spi.Common.Context.Definitions;
using Dfe.Spi.Common.Http.Server;
using Dfe.Spi.Common.Http.Server.Definitions;
using Dfe.Spi.Common.Logging;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Application.LearningProviders;
using Dfe.Spi.Search.Application.ManagementGroups;
using Dfe.Spi.Search.Domain.Configuration;
using Dfe.Spi.Search.Domain.LearningProviders;
using Dfe.Spi.Search.Domain.ManagementGroups;
using Dfe.Spi.Search.Functions;
using Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch;
using Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.LearningProviders;
using Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.ManagementGroups;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Dfe.Spi.Search.Functions
{
    public class Startup : FunctionsStartup
    {
        private IConfigurationRoot _rawConfiguration;
        private SearchConfiguration _configuration;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            LoadAndAddConfiguration(services);
            AddLogging(services);
            AddLearningProviders(services);
            AddManagementGroups(services);
        }
        
        private void LoadAndAddConfiguration(IServiceCollection services)
        {
            _rawConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables(prefix: "SPI_")
                .Build();
            services.AddSingleton(_rawConfiguration);

            _configuration = new SearchConfiguration();
            _rawConfiguration.Bind(_configuration);
            services.AddSingleton(_configuration);
            services.AddSingleton(_configuration.SearchIndex);
        }
        
        private void AddLogging(IServiceCollection services)
        {
            services.AddLogging();
            services.AddScoped<ILogger>(provider =>
                provider.GetService<ILoggerFactory>().CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));
            services.AddScoped<ILoggerWrapper, LoggerWrapper>();
            
            services.AddScoped<IHttpSpiExecutionContextManager, HttpSpiExecutionContextManager>();
            services.AddScoped<ISpiExecutionContextManager>((provider) =>
                (ISpiExecutionContextManager) provider.GetService(typeof(IHttpSpiExecutionContextManager)));
            services.AddScoped<ILoggerWrapper, LoggerWrapper>();
        }

        private void AddLearningProviders(IServiceCollection services)
        {
            services.AddScoped<ILearningProviderSearchIndex, AcsLearningProviderSearchIndex>();
            services.AddScoped<ILearningProviderSearchManager, LearningProviderSearchManager>();
        }

        private void AddManagementGroups(IServiceCollection services)
        {
            services.AddScoped<IManagementGroupSearchIndex, AcsManagementGroupSearchIndex>();
            services.AddScoped<IManagementGroupSearchManager, ManagementGroupSearchManager>();
        }
    }
}