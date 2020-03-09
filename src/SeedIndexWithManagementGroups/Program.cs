using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Search.Application.ManagementGroups;
using Dfe.Spi.Search.Domain.Configuration;
using Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.ManagementGroups;
using Newtonsoft.Json;

namespace SeedIndexWithManagementGroups
{
    class Program
    {
        private static Logger _logger;
        private static IManagementGroupSearchManager _searchManager;

        static async Task Run(CommandLineOptions options, CancellationToken cancellationToken = default)
        {
            Init(options);

            var managementGroups = await ReadManagementGroups(options.InputPath, cancellationToken);
            await SyncManagementGroups(managementGroups, options.Source, cancellationToken);
        }

        static void Init(CommandLineOptions options)
        {
            var searchIndex = new AcsManagementGroupSearchIndex(
                new SearchIndexConfiguration
                {
                    AzureCognitiveSearchServiceName = options.AcsServiceName,
                    AzureCognitiveSearchKey = options.AcsKey,
                    ManagementGroupIndexName = options.IndexName,
                },
                _logger);
            _searchManager = new ManagementGroupSearchManager(searchIndex, _logger);
        }

        static async Task<ManagementGroup[]> ReadManagementGroups(string path, CancellationToken cancellationToken)
        {
            using(var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<ManagementGroup[]>(json);
            }
        }

        static async Task SyncManagementGroups(ManagementGroup[] managementGroups, string source,
            CancellationToken cancellationToken)
        {
            const int batchSize = 100;
            var position = 0;

            while (position < managementGroups.Length)
            {
                var batch = managementGroups.Skip(position).Take(batchSize).ToArray();
                _logger.Info($"Starting batch {position} - {position + batch.Length} of {managementGroups.Length}");

                await _searchManager.SyncBatchAsync(batch, source, cancellationToken);

                position += batchSize;
            }
        }
        
        
        static void Main(string[] args)
        {
            _logger = new Logger();

            CommandLineOptions options = null;
            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed((parsed) => options = parsed);
            if (options != null)
            {
                try
                {
                    Run(options).Wait();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                _logger.Info("Done. Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}