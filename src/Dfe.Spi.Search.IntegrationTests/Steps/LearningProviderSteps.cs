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
using Dfe.Spi.Search.IntegrationTests.TestHelpers;
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

        [Given("the index contains matching documents")]
        public void GivenTheIndexContainsMatchingDocuments()
        {
            var index = _host.GetInstanceOfLearningProviderSearchIndex();
            index.SetIndexDataset(new []
            {
                new LearningProviderSearchDocument
                {
                    Name = "Example Provider one",
                    SourceSystemName = "Test",
                    SourceSystemId = "T001",
                }, 
            });
        }
        
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
            var request = RequestBuilder.CreateRequest()
                .WithMethod("POST")
                .WithJsonBody(new SearchRequest {Filter = _searchFilters.ToArray()})
                .AsHttpRequest();
            var cancellationToken = default(CancellationToken);
            var actual = await function.Run(request, cancellationToken);
            
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<OkObjectResult>(actual);
            Assert.IsInstanceOf<SearchResultset<LearningProviderSearchDocument>>(((OkObjectResult)actual).Value);
            Assert.AreEqual(1, ((SearchResultset<LearningProviderSearchDocument>)((OkObjectResult)actual).Value).Documents?.Length);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var logger = _host.GetInstanceOfLogger();
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