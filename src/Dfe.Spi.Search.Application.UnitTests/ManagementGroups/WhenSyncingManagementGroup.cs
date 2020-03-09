using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.UnitTesting.Fixtures;
using Dfe.Spi.Common.WellKnownIdentifiers;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Search.Application.ManagementGroups;
using Dfe.Spi.Search.Domain.ManagementGroups;
using Moq;
using NUnit.Framework;

namespace Dfe.Spi.Search.Application.UnitTests.ManagementGroups
{
    public class WhenSyncingManagementGroup
    {
        private Mock<IManagementGroupSearchIndex> _searchIndexMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private ManagementGroupSearchManager _manager;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _searchIndexMock = new Mock<IManagementGroupSearchIndex>();

            _loggerMock = new Mock<ILoggerWrapper>();

            _manager = new ManagementGroupSearchManager(_searchIndexMock.Object, _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldUploadBatchWithSingleDocument(ManagementGroup managementGroup)
        {
            await _manager.SyncAsync(managementGroup, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<ManagementGroupSearchDocument[]>(a => a.Length == 1),
                    _cancellationToken),
                Times.Once);
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldMapSourceNameFromSpecifiedSource(ManagementGroup managementGroup)
        {
            await _manager.SyncAsync(managementGroup, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<ManagementGroupSearchDocument[]>(a =>
                        a.Length == 1 &&
                        a[0].SourceSystemName == SourceSystemNames.GetInformationAboutSchools),
                    _cancellationToken),
                Times.Once);
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldMapSourceIdFromCode(ManagementGroup managementGroup)
        {
            await _manager.SyncAsync(managementGroup, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<ManagementGroupSearchDocument[]>(a =>
                        a.Length == 1 &&
                        a[0].SourceSystemId == managementGroup.Code),
                    _cancellationToken),
                Times.Once);
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldMapSearchableFieldsFromManagementGroup(ManagementGroup managementGroup)
        {
            await _manager.SyncAsync(managementGroup, SourceSystemNames.GetInformationAboutSchools,
                _cancellationToken);

            _searchIndexMock.Verify(i => i.UploadBatchAsync(
                    It.Is<ManagementGroupSearchDocument[]>(a =>
                        a.Length == 1 &&
                        a[0].Name == managementGroup.Name &&
                        a[0].Code == managementGroup.Code &&
                        a[0].Type == managementGroup.Type &&
                        a[0].Identifier == managementGroup.Identifier &&
                        a[0].CompaniesHouseNumber == managementGroup.CompaniesHouseNumber),
                    _cancellationToken),
                Times.Once);
        }
    }
}