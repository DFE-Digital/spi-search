using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Http.Server.Definitions;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Search.Application.LearningProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dfe.Spi.Search.Functions.LearningProviders
{
    public class SyncLearningProvider
    {
        private const string FunctionName = nameof(SyncLearningProvider);

        private readonly ILearningProviderSearchManager _searchManager;
        private readonly ILoggerWrapper _logger;
        private readonly IHttpSpiExecutionContextManager _spiExecutionContextManager;

        public SyncLearningProvider(
            ILearningProviderSearchManager searchManager, 
            ILoggerWrapper logger,
            IHttpSpiExecutionContextManager spiExecutionContextManager)
        {
            _searchManager = searchManager;
            _logger = logger;
            _spiExecutionContextManager = spiExecutionContextManager;
        }

        [FunctionName(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "learning-providers/sync/{source}")]
            HttpRequest req,
            string source,
            CancellationToken cancellationToken)
        {
            _spiExecutionContextManager.SetContext(req.Headers);
            _logger.Info($"Start processing sync of learning provider from {source}...");

            LearningProvider learningProvider;
            using (var reader = new StreamReader(req.Body))
            {
                var json = await reader.ReadToEndAsync();
                _logger.Debug($"Received body {json}");

                learningProvider = JsonConvert.DeserializeObject<LearningProvider>(json);
                _logger.Info($"Received learning provider for sync: {JsonConvert.SerializeObject(learningProvider)}");
            }

            await _searchManager.SyncAsync(learningProvider, source, cancellationToken);
            _logger.Info("Successfully sync'd learning provider");

            return new AcceptedResult();
        }
    }
}