using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Application;
using Dfe.Spi.Search.Application.LearningProviders;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.LearningProviders;
using Dfe.Spi.Search.Functions.LearningProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dfe.Spi.Search.Functions.UnitTests.LearningProviders
{
    public class WhenSearchingLearningProviders
    {
        private Mock<ILearningProviderSearchManager> _searchManagerMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private DefaultHttpRequest _httpRequest;
        private CancellationToken _cancellationToken;
        private SearchLearningProviders _function;

        [SetUp]
        public void Arrange()
        {
            _searchManagerMock = new Mock<ILearningProviderSearchManager>();
            _searchManagerMock.Setup(m => m.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SearchResultset<LearningProviderSearchDocument>
                {
                    Documents = new LearningProviderSearchDocument[0]
                });

            _loggerMock = new Mock<ILoggerWrapper>();

            _function = new SearchLearningProviders(_searchManagerMock.Object, _loggerMock.Object);

            _httpRequest = new DefaultHttpRequest(new DefaultHttpContext());

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldSetLoggerContext()
        {
            await _function.Run(_httpRequest, _cancellationToken);

            _loggerMock.Verify(l => l.SetContext(_httpRequest.Headers), 
                Times.Once);
        }

        [Test, AutoData]
        public async Task ThenItShouldQuerySearchManagerWithSearchRequestDeserializedFromRequestBody(
            SearchRequest request)
        {
            _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));

            await _function.Run(_httpRequest, _cancellationToken);
            
            _searchManagerMock.Verify(m=>m.SearchAsync(It.Is<SearchRequest>(actual => AreEqual(request, actual)), _cancellationToken), 
                Times.Once);
        }

        [Test, AutoData]
        public async Task ThenItShouldReturnResultsFromSearchManager(SearchResultset<LearningProviderSearchDocument> searchResultset)
        {
            _searchManagerMock.Setup(m => m.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResultset);

            var actual = await _function.Run(_httpRequest, _cancellationToken);
            
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<OkObjectResult>(actual);
            Assert.AreSame(searchResultset, ((OkObjectResult)actual).Value);
        }

        [Test]
        public async Task ThenItShouldReturnBadRequestIfSearchManageThrowsInvalidRequestException()
        {
            _searchManagerMock.Setup(m => m.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidRequestException("Unit test reason 1", "Unit test reason 2"));

            var actual = await _function.Run(_httpRequest, _cancellationToken);
            
            
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<BadRequestObjectResult>(actual);
        }

        private bool AreEqual(SearchRequest expected, SearchRequest actual)
        {
            if (expected?.Filter?.Length != actual?.Filter?.Length)
            {
                return false;
            }

            for (var i = 0; i < expected.Filter.Length; i++)
            {
                var expectedFilter = expected.Filter[i];
                var actualFilter = actual.Filter[i];

                if (expectedFilter.Field != actualFilter.Field
                    || expectedFilter.Value != actualFilter.Value)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}