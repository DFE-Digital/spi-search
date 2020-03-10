using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Http.Server.Definitions;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.Common.UnitTesting.Fixtures;
using Dfe.Spi.Models.Entities;
using Dfe.Spi.Search.Application.ManagementGroups;
using Dfe.Spi.Search.Functions.ManagementGroups;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dfe.Spi.Search.Functions.UnitTests.ManagementGroups
{
    public class WhenSyncingManagementGroup
    {
        private Mock<IManagementGroupSearchManager> _searchManagerMock;
        private Mock<ILoggerWrapper> _loggerMock;
        private Mock<IHttpSpiExecutionContextManager> _spiExecutionManagerMock;
        private SyncManagementGroup _function;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _searchManagerMock = new Mock<IManagementGroupSearchManager>();

            _loggerMock = new Mock<ILoggerWrapper>();
            
            _spiExecutionManagerMock = new Mock<IHttpSpiExecutionContextManager>();

            _function = new SyncManagementGroup(
                _searchManagerMock.Object,
                _loggerMock.Object,
                _spiExecutionManagerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldSyncDeserializedManagementGroup(ManagementGroup managementGroup, string source)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(managementGroup))),
            };

            await _function.RunAsync(request, source, _cancellationToken);

            _searchManagerMock.Verify(m => m.SyncAsync(
                    It.Is<ManagementGroup>(lp =>
                        lp.Name == managementGroup.Name &&
                        lp.Code == managementGroup.Code &&
                        lp.Type == managementGroup.Type &&
                        lp.Identifier == managementGroup.Identifier &&
                        lp.CompaniesHouseNumber == managementGroup.CompaniesHouseNumber),
                    source,
                    _cancellationToken),
                Times.Once());
        }

        [Test, NonRecursiveAutoData]
        public async Task ThenItShouldReturnAcceptedResult(ManagementGroup managementGroup, string source)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(managementGroup))),
            };

            var actual = await _function.RunAsync(request, source, _cancellationToken);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<AcceptedResult>(actual);
        }
    }
}