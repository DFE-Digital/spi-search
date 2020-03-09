using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Search.Application.ManagementGroups;
using Dfe.Spi.Search.Domain.Common;
using Dfe.Spi.Search.Domain.ManagementGroups;
using Moq;
using NUnit.Framework;

namespace Dfe.Spi.Search.Application.UnitTests.ManagementGroups
{
    public class WhenSearchingForManagementGroups
    {
        private Mock<IManagementGroupSearchIndex> _searchIndexMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private ManagementGroupSearchManager _manager;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _searchIndexMock = new Mock<IManagementGroupSearchIndex>();
            _searchIndexMock.Setup(i => i.GetSearchableFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] {"Name", "Type", "OpenDate"});

            _loggerMock = new Mock<ILoggerWrapper>();

            _manager = new ManagementGroupSearchManager(_searchIndexMock.Object, _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldSearchIndexWithProvidedRequest()
        {
            var request = new SearchRequest
            {
                Groups = new []
                {
                    new SearchGroup
                    {
                        Filter = new[]
                        {
                            new SearchFilter {Field = "Name"},
                        },
                        CombinationOperator = "and"
                    }, 
                },
                CombinationOperator = "and"
            };

            await _manager.SearchAsync(request, _cancellationToken);

            _searchIndexMock.Verify(i => i.SearchAsync(request, _cancellationToken),
                Times.Once);
        }

        [TestCase("Name", Operators.Contains)]
        [TestCase("Type", Operators.Equals)]
        public async Task ThenItShouldDefaultFilterOperatorIfNotSpecified(string field, string defaultOperator)
        {
            var request = new SearchRequest
            {
                Groups = new []
                {
                    new SearchGroup
                    {
                        Filter = new[]
                        {
                            new SearchFilter {Field = field},
                        },
                        CombinationOperator = "and"
                    }, 
                },
                CombinationOperator = "and"
            };

            await _manager.SearchAsync(request, _cancellationToken);

            _searchIndexMock.Verify(i => i.SearchAsync(
                    It.Is<SearchRequest>(r=>
                        r.Groups[0].Filter[0].Operator == defaultOperator), 
                    _cancellationToken),
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
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchRequestHasNoGroups()
        {
            var request = new SearchRequest
            {
                Groups = null,
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "Must provide groups");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("both")]
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchRequestHasMissingOrInvalidCombinationOperator(string requestCombinationOperator)
        {
            var request = new SearchRequest
            {
                Groups = new []
                {
                    new SearchGroup
                    {
                        Filter = null,
                        CombinationOperator = "and",
                    }, 
                },
                CombinationOperator = requestCombinationOperator,
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "Request combinationOperator must be either 'and' or 'or'");
        }

        [Test]
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchGroupHasNoFilters()
        {
            var request = new SearchRequest
            {
                Groups = new []
                {
                    new SearchGroup
                    {
                        Filter = null,
                        CombinationOperator = "and",
                    }, 
                },
                CombinationOperator = "and",
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "Group 0 must have filters");
            _searchIndexMock.Verify(i => i.GetSearchableFieldsAsync(_cancellationToken),
                Times.Once);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("both")]
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchGroupHasMissingOrInvalidCombinationOperator(string groupCombinationOperator)
        {
            var request = new SearchRequest
            {
                Groups = new []
                {
                    new SearchGroup
                    {
                        Filter = null,
                        CombinationOperator = groupCombinationOperator,
                    }, 
                },
                CombinationOperator = "and",
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "Group 0 combinationOperator must be either 'and' or 'or'");
            _searchIndexMock.Verify(i => i.GetSearchableFieldsAsync(_cancellationToken),
                Times.Once);
        }

        [Test]
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchRequestHasFilterWithInvalidFieldName()
        {
            var request = new SearchRequest
            {
                Groups = new []
                {
                    new SearchGroup
                    {
                        Filter = new[]
                        {
                            new SearchFilter {Field = "SomeField"},
                            new SearchFilter {Field = "AnotherField"},
                            new SearchFilter {Field = "Name"},
                        },
                        CombinationOperator = "and",
                    }, 
                },
                CombinationOperator = "and",
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "SomeField in group 0 is not a valid field for filtering");
            AssertInvalidRequestHasReason(actual, "AnotherField in group 0 is not a valid field for filtering");
            _searchIndexMock.Verify(i => i.GetSearchableFieldsAsync(_cancellationToken),
                Times.Once);
        }

        [Test]
        public void ThenItShouldThrowInvalidRequestExceptionIfSearchRequestHasFilterWithInvalidOperator()
        {
            var request = new SearchRequest
            {
                Groups = new []
                {
                    new SearchGroup
                    {
                        Filter = new[]
                        {
                            new SearchFilter {Field = "Name", Operator = Operators.GreaterThan},
                        },
                        CombinationOperator = "and",
                    }, 
                },
                CombinationOperator = "and",
            };

            var actual = Assert.ThrowsAsync<InvalidRequestException>(async () =>
                await _manager.SearchAsync(request, _cancellationToken));
            AssertInvalidRequestHasReason(actual, "Operator greaterthan is not valid for Name in group 0");
            _searchIndexMock.Verify(i => i.GetSearchableFieldsAsync(_cancellationToken),
                Times.Once);
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