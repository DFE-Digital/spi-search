using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.UnitTesting.Fixtures;
using Dfe.Spi.Common.WellKnownIdentifiers;
using Dfe.Spi.Models;
using Dfe.Spi.Search.Application.LearningProviders;
using Dfe.Spi.Search.Domain.LearningProviders;
using Moq;
using NUnit.Framework;

namespace Dfe.Spi.Search.Application.UnitTests.LearningProviders
{
    public class WhenSyncingLearningProvider
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

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldUploadBatchWithSingleDocument(LearningProvider learningProvider)
        {
            await _manager.SyncAsync(learningProvider, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<LearningProviderSearchDocument[]>(a => a.Length == 1),
                    _cancellationToken),
                Times.Once);
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldMapSourceNameromSpecifiedSource(LearningProvider learningProvider)
        {
            await _manager.SyncAsync(learningProvider, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<LearningProviderSearchDocument[]>(a =>
                        a.Length == 1 &&
                        a[0].SourceSystemName == SourceSystemNames.GetInformationAboutSchools),
                    _cancellationToken),
                Times.Once);
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldMapSourceIdFromUrnForGias(LearningProvider learningProvider)
        {
            await _manager.SyncAsync(learningProvider, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<LearningProviderSearchDocument[]>(a =>
                        a.Length == 1 &&
                        a[0].SourceSystemId == learningProvider.Urn.ToString()),
                    _cancellationToken),
                Times.Once);
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldMapSourceIdFromUkprnForUkrlp(LearningProvider learningProvider)
        {
            await _manager.SyncAsync(learningProvider, SourceSystemNames.UkRegisterOfLearningProviders,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<LearningProviderSearchDocument[]>(a =>
                        a.Length == 1 &&
                        a[0].SourceSystemId == learningProvider.Ukprn.ToString()),
                    _cancellationToken),
                Times.Once);
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldMapSearchableFieldsFromLearningProvider(LearningProvider learningProvider)
        {
            await _manager.SyncAsync(learningProvider, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<LearningProviderSearchDocument[]>(a =>
                        a.Length == 1 &&
                        a[0].Name == learningProvider.Name),
                    _cancellationToken),
                Times.Once);
        }
    }
}