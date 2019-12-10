using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.LearningProviders;
using Dfe.Spi.Search.Functions.LearningProviders;
using Dfe.Spi.Search.IntegrationTests.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Dfe.Spi.Search.IntegrationTests.Steps
{
    [Binding]
    public class LearningProviderSteps
    {
        private TestFunctionHost _host = new TestFunctionHost();
        private List<SearchFilter> _searchFilters = new List<SearchFilter>();
        
        [When("I search for Learning Providers by name")]
        public void WhenISearchForLearningProvidersByName()
        {
            _searchFilters.Add(new SearchFilter
            {
                Field = "Name",
                Value = "Example"
            });
        }

        [Then("I should receive search results")]
        public async Task ThenIShouldReceiveSearchResults()
        {
            var function = _host.GetInstance<SearchLearningProviders>();
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SearchRequest{Filter = _searchFilters.ToArray()})));
            var cancellationToken = default(CancellationToken);
            var actual = await function.Run(request, cancellationToken);
            
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<OkObjectResult>(actual);
            Assert.IsInstanceOf<SearchResultset<LearningProviderSearchDocument>>(((OkObjectResult)actual).Value);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var logger = (ConsoleLoggerWrapper) _host.GetInstance<ILoggerWrapper>();
            var logs = logger.GetLogs();
            
            Console.WriteLine();
            Console.WriteLine("Log papertrail for test:");
            foreach (var log in logs)
            {
                Console.WriteLine(log);
            }
        }
    }
}