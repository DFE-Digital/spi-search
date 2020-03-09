using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Http.Server.Definitions;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Search.Application.ManagementGroups;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Dfe.Spi.Search.Functions.ManagementGroups
{
    public class SyncManagementGroup
    {
        private const string FunctionName = nameof(SyncManagementGroup);

        private readonly IManagementGroupSearchManager _searchManager;
        private readonly ILoggerWrapper _logger;
        private readonly IHttpSpiExecutionContextManager _spiExecutionContextManager;

        public SyncManagementGroup(
            IManagementGroupSearchManager searchManager, 
            ILoggerWrapper logger,
            IHttpSpiExecutionContextManager spiExecutionContextManager)
        {
            _searchManager = searchManager;
            _logger = logger;
            _spiExecutionContextManager = spiExecutionContextManager;
        }

        [FunctionName(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "management-groups/sync/{source}")]
            HttpRequest req,
            string source,
            CancellationToken cancellationToken)
        {
            _spiExecutionContextManager.SetContext(req.Headers);
            _logger.Info($"Start processing sync of management group from {source}...");

            ManagementGroup managementGroup;
            using (var reader = new StreamReader(req.Body))
            {
                var json = await reader.ReadToEndAsync();
                _logger.Debug($"Received body {json}");

                managementGroup = JsonConvert.DeserializeObject<ManagementGroup>(json);
                _logger.Info($"Received management group for sync: {JsonConvert.SerializeObject(managementGroup)}");
            }

            await _searchManager.SyncAsync(managementGroup, source, cancellationToken);
            _logger.Info("Successfully sync'd management group");

            return new AcceptedResult();
        }
    }
}