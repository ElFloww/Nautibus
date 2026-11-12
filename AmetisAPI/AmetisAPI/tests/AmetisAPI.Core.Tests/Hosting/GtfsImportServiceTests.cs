using AmetisAPI.Core.Hosting;
using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;

namespace AmetisAPI.Core.Tests.Hosting
{
    public class GtfsImportServiceTests
    {
        private readonly Mock<IMediator> m_Mediator = new();

        private GtfsImportService Service() => new(Mock.Of<IServiceScopeFactory>(), Mock.Of<ILogger<GtfsImportService>>());

        private void Imports(bool p_RealtimeSucceeds)
        {
            m_Mediator.Setup(p_M => p_M.SendQueryAsync<ImportGtfsFeedRequest, ValueResponse<FeedVersionInfo>>(It.Is<ImportGtfsFeedRequest>(p_R => p_R.Source == FeedSource.Network), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValueResponse<FeedVersionInfo>((FeedVersionInfo)null));
            m_Mediator.Setup(p_M => p_M.SendQueryAsync<ImportGtfsFeedRequest, ValueResponse<FeedVersionInfo>>(It.Is<ImportGtfsFeedRequest>(p_R => p_R.Source == FeedSource.Realtime), It.IsAny<CancellationToken>()))
                .ReturnsAsync(p_RealtimeSucceeds ? new ValueResponse<FeedVersionInfo>((FeedVersionInfo)null) : ValueResponse<FeedVersionInfo>.Error(DomainErrors.Gtfs.DownloadFailed, "coupure"));
        }

        [Fact]
        public async Task Failed_Import_Is_Retried_After_Ten_Minutes_Then_Back_To_Six_Hours()
        {
            GtfsImportService v_Service = Service();

            Imports(p_RealtimeSucceeds: false);
            await v_Service.RunOnceAsync(m_Mediator.Object, CancellationToken.None);
            Check.That(v_Service.NextDelay).IsEqualTo(TimeSpan.FromMinutes(10));

            Imports(p_RealtimeSucceeds: true);
            await v_Service.RunOnceAsync(m_Mediator.Object, CancellationToken.None);
            Check.That(v_Service.NextDelay).IsEqualTo(TimeSpan.FromHours(6));
        }
    }
}
