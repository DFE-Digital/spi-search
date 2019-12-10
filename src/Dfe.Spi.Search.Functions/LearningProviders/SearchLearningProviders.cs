using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Application;
using Dfe.Spi.Search.Application.LearningProviders;
using Dfe.Spi.Search.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Dfe.Spi.Search.Functions.LearningProviders
{
    public class SearchLearningProviders
    {
        private const string FunctionName = nameof(SearchLearningProviders);

        private readonly ILearningProviderSearchManager _searchManager;
        private readonly ILoggerWrapper _logger;

        public SearchLearningProviders(ILearningProviderSearchManager searchManager, ILoggerWrapper logger)
        {
            _searchManager = searchManager;
            _logger = logger;
        }

        [FunctionName(FunctionName)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "learning-providers")]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            _logger.SetContext(req.Headers);
            _logger.Info($"Start processing search for learning providers...");

            SearchRequest searchRequest;
            using (var reader = new StreamReader(req.Body))
            {
                var json = await reader.ReadToEndAsync();
                searchRequest = JsonConvert.DeserializeObject<SearchRequest>(json);
            }

            _logger.Info($"Received search request {JsonConvert.SerializeObject(searchRequest)}");

            try
            {
                var results = await _searchManager.SearchAsync(searchRequest, cancellationToken);
                _logger.Info($"Received {results.Documents.Length} documents in resultset");

                return new OkObjectResult(results);
            }
            catch (InvalidRequestException ex)
            {
                _logger.Info($"Request was invalid: {JsonConvert.SerializeObject(ex.Reasons)}");
                return new BadRequestObjectResult(new
                {
                    ex.Reasons,
                });
            }
        }
    }
}