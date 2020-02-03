using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.UnitTesting.Fixtures;
using Dfe.Spi.Models;
using Dfe.Spi.Search.Application.LearningProviders;
using Dfe.Spi.Search.Functions.LearningProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dfe.Spi.Search.Functions.UnitTests.LearningProviders
{
    public class WhenSyncingLearningProvider
    {
        private Mock<ILearningProviderSearchManager> _searchManagerMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private SyncLearningProvider _function;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _searchManagerMock = new Mock<ILearningProviderSearchManager>();

            _loggerMock = new Mock<ILoggerWrapper>();

            _function = new SyncLearningProvider(
                _searchManagerMock.Object,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldSyncDeserializedLearningProvider(LearningProvider learningProvider, string source)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(learningProvider))),
            };

            await _function.RunAsync(request, source, _cancellationToken);

            _searchManagerMock.Verify(m => m.SyncAsync(
                    It.Is<LearningProvider>(lp =>
                        lp.Urn == learningProvider.Urn &&
                        lp.Ukprn == learningProvider.Ukprn &&
                        lp.Name == learningProvider.Name),
                    source,
                    _cancellationToken),
                Times.Once());
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldReturnAcceptedResult(LearningProvider learningProvider, string source)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(learningProvider))),
            };

            var actual = await _function.RunAsync(request, source, _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<AcceptedResult>(actual);
        }
    }
}