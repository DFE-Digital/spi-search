using System.Threading;
using System.Threading.Tasks;
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
                        a[0].Name == learningProvider.Name &&
                        a[0].Urn == learningProvider.Urn &&
                        a[0].Ukprn == learningProvider.Ukprn &&
                        a[0].Uprn == learningProvider.Uprn &&
                        a[0].CompaniesHouseNumber == learningProvider.CompaniesHouseNumber &&
                        a[0].CharitiesCommissionNumber == learningProvider.CharitiesCommissionNumber &&
                        a[0].AcademyTrustCode == learningProvider.AcademyTrustCode &&
                        a[0].DfeNumber == learningProvider.DfeNumber &&
                        a[0].EstablishmentNumber == learningProvider.EstablishmentNumber &&
                        a[0].PreviousEstablishmentNumber == learningProvider.PreviousEstablishmentNumber),
                    _cancellationToken),
                Times.Once);
        }
    }
}