using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Application.LearningProviders;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.LearningProviders;
using Moq;
using NUnit.Framework;

namespace Dfe.Spi.Search.Application.UnitTests.LearningProviders
{
    public class WhenSearchingForLearningProviders
    {
        private Mock<ILearningProviderSearchIndex> _searchIndexMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private LearningProviderSearchManager _manager;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _searchIndexMock = new Mock<ILearningProviderSearchIndex>();

            _loggerMock = new Mock<ILoggerWrapper>();

            _manager = new LearningProviderSearchManager(_searchIndexMock.Object, _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldSearchIndexWithProvidedRequest()
        {
            var request = new SearchRequest
            {
                Filter = new[]
                {
                    new SearchFilter {Field = "Name"},
                },
            };
            
            await _manager.SearchAsync(request, _cancellationToken);

            _searchIndexMock.Verify(i => i.SearchAsync(request, _cancellationToken),
                Times.Once);
        }

        [Test]
        public void ThenItShouldThrowInvalidRequestExceptionIfNoSearchRequestProvided()
        {
            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(null, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "Must provide SearchRequest");
        }

        [Test]
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchRequestHasNoFilters()
        {
            var request = new SearchRequest
            {
                Filter = null,
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "Must provide filters");
        }

        [Test]
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchRequestHasFilterWithInvalidFieldName()
        {
            var request = new SearchRequest
            {
                Filter = new[]
                {
                    new SearchFilter {Field = "SomeField"},
                    new SearchFilter {Field = "AnotherField"},
                    new SearchFilter {Field = "Name"},
                },
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "SomeField is not a valid field for filtering");
            AssertInvalidRequestHasReason(actual, "AnotherField is not a valid field for filtering");
        }

        private void AssertInvalidRequestHasReason(InvalidRequestException ex, string expectedReason)
        {
            var reasonsString = ex.Reasons == null
                ? "(null)"
                : ex.Reasons.Select(x => $"  > {x}").Aggregate((x, y) => $"{x}\n{y}");
            Assert.IsNotNull(ex.Reasons?.SingleOrDefault(r => r == expectedReason),
                $"Could not find expected reason {expectedReason}\nReasons:\n{reasonsString}\n");
        }
    }
}