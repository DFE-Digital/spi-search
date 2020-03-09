using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dfe.Spi.Search.Domain.Configuration;
using Dfe.Spi.Search.Domain.LearningProviders;
using Dfe.Spi.Search.Infrastructure.AzureCognitiveSearch.LearningProviders;

namespace GiasDataLoader
{
    class Program
    {
        private static Logger _logger;

        static async Task Run(CommandLineOptions options, CancellationToken cancellationToken = default)
        {
            var searchIndexConfig = new SearchIndexConfiguration
            {
                AzureCognitiveSearchServiceName = options.SearchServiceName,
                AzureCognitiveSearchKey = options.SearchServiceKey,
                LearningProviderIndexName = options.IndexName,
            };

            await ProcessEstablishmentsFile(options, searchIndexConfig, cancellationToken);
        }

        static async Task ProcessEstablishmentsFile(CommandLineOptions options,
            SearchIndexConfiguration searchIndexConfig, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(options.EstablishmentsFilePath))
            {
                return;
            }

            _logger.Info($"Reading establishments from {options.EstablishmentsFilePath}");
            Establishment[] establishments;
            using (var stream = new FileStream(options.EstablishmentsFilePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvFileParser<Establishment>(reader, new EstablishmentCsvMapping()))
            {
                establishments = parser.GetRecords();
            }

            _logger.Info($"Found {establishments.Length} establishments");

            var index = new AcsLearningProviderSearchIndex(searchIndexConfig, _logger);

            _logger.Info($"Creating index {options.IndexName}");
            await index.CreateOrUpdateIndexAsync(cancellationToken);
            var skip = 0;
            var take = 50;
            while (skip < establishments.Length)
            {
                var batch = establishments
                    .Skip(skip)
                    .Take(take)
                    .Select(e => new LearningProviderSearchDocument
                    {
                        Name = e.Name,
                        SourceSystemName = "GIAS",
                        SourceSystemId = e.Urn.ToString(),
                    })
                    .ToArray();

                _logger.Info($"Uploading batch of documents {skip} - {skip + take} of {establishments.Length}");
                await index.UploadBatchAsync(batch, cancellationToken);

                skip += take;
            }
            _logger.Info("Finished uploading establishments to index");
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