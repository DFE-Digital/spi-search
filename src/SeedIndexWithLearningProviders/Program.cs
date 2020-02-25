using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Search.Application.LearningProviders;
using Dfe.Spi.Search.Domain.Configuration;
using Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.LearningProviders;
using Newtonsoft.Json;

namespace SeedIndexWithLearningProviders
{
    class Program
    {
        private static Logger _logger;
        private static ILearningProviderSearchManager _searchManager;

        static async Task Run(CommandLineOptions options, CancellationToken cancellationToken = default)
        {
            Init(options);

            var learningProviders = await ReadLearningProviders(options.InputPath, cancellationToken);
            await SyncLearningProviders(learningProviders, options.Source, cancellationToken);
        }

        static void Init(CommandLineOptions options)
        {
            var searchIndex = new AcsLearningProviderSearchIndex(
                new SearchIndexConfiguration
                {
                    AzureCognitiveSearchServiceName = options.AcsServiceName,
                    AzureCognitiveSearchKey = options.AcsKey,
                    IndexName = options.IndexName,
                },
                _logger);
            _searchManager = new LearningProviderSearchManager(searchIndex, _logger);
        }

        static async Task<LearningProvider[]> ReadLearningProviders(string path, CancellationToken cancellationToken)
        {
            using(var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<LearningProvider[]>(json);
            }
        }

        static async Task SyncLearningProviders(LearningProvider[] learningProviders, string source,
            CancellationToken cancellationToken)
        {
            const int batchSize = 100;
            var position = 0;

            while (position < learningProviders.Length)
            {
                var batch = learningProviders.Skip(position).Take(batchSize).ToArray();
                _logger.Info($"Starting batch {position} - {position + batch.Length} of {learningProviders.Length}");

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