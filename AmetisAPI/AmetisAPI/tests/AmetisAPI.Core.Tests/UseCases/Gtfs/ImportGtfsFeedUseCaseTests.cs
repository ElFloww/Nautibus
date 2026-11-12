using AmetisAPI.Core.Requests.Gtfs;
using AmetisAPI.Core.Tests.Utilities;
using AmetisAPI.Core.UseCases.Gtfs;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Exceptions;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Gtfs;

namespace AmetisAPI.Core.Tests.UseCases.Gtfs
{
    public class ImportGtfsFeedUseCaseTests
    {
        private static readonly byte[] m_Content = [1, 2, 3];
        private static readonly string m_Hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(m_Content));

        private readonly Mock<IGtfsSource> m_Source = new();
        private readonly Mock<IGtfsParser> m_Parser = new();
        private readonly Mock<IFeedRepository> m_Feeds = new();

        public ImportGtfsFeedUseCaseTests()
        {
            m_Source.Setup(p_S => p_S.GetUrl(FeedSource.Network)).Returns("https://exemple/gtfs.zip");
            m_Source.Setup(p_S => p_S.DownloadAsync(FeedSource.Network, It.IsAny<CancellationToken>())).ReturnsAsync(m_Content);
            m_Feeds.Setup(p_F => p_F.StartVersionAsync(FeedSource.Network, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(7);
            m_Feeds.Setup(p_F => p_F.GetAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(Info(7, FeedImportStatus.Succeeded, m_Hash));
        }

        private static FeedVersionInfo Info(int p_Id, FeedImportStatus p_Status, string p_Hash)
            => new(p_Id, FeedSource.Network, p_Status, true, p_Hash, DateTime.UtcNow, null, 1, 3, 2, 4, 0, null, null, null, null);

        private Task<ValueResponse<FeedVersionInfo>> RunAsync()
            => new ImportGtfsFeedUseCase(Mock.Of<ILogger<ImportGtfsFeedUseCase>>(), Mock.Of<IUnitOfWork>(), m_Source.Object, m_Parser.Object, m_Feeds.Object)
                .Handle(new ImportGtfsFeedRequest(FeedSource.Network), CancellationToken.None);

        [Fact]
        public async Task Same_Content_As_Active_Version_Is_Skipped_Without_Writing()
        {
            m_Feeds.Setup(p_F => p_F.GetActiveAsync(FeedSource.Network, It.IsAny<CancellationToken>())).ReturnsAsync(Info(3, FeedImportStatus.Succeeded, m_Hash));

            ValueResponse<FeedVersionInfo> v_Result = await RunAsync();

            Check.That(v_Result.Success).IsTrue();
            m_Feeds.Verify(p_F => p_F.MarkSkippedAsync(7, It.IsAny<CancellationToken>()), Times.Once);
            m_Feeds.Verify(p_F => p_F.WriteContentAsync(It.IsAny<int>(), It.IsAny<GtfsFeed>(), It.IsAny<CancellationToken>()), Times.Never);
            m_Parser.Verify(p_P => p_P.Parse(It.IsAny<Stream>()), Times.Never);
        }

        [Fact]
        public async Task New_Content_Is_Written_Then_Activated_With_Change_Summary()
        {
            m_Feeds.Setup(p_F => p_F.GetActiveAsync(FeedSource.Network, It.IsAny<CancellationToken>())).ReturnsAsync(Info(3, FeedImportStatus.Succeeded, "ANCIEN"));
            m_Feeds.Setup(p_F => p_F.GetRouteIdsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(["L1", "L9"]);
            m_Feeds.Setup(p_F => p_F.GetStopIdsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(["AREA", "Q1", "Q2"]);
            m_Parser.Setup(p_P => p_P.Parse(It.IsAny<Stream>())).Returns(GtfsFeedBuilder.Valid());

            ValueResponse<FeedVersionInfo> v_Result = await RunAsync();

            Check.That(v_Result.Success).IsTrue();
            m_Feeds.Verify(p_F => p_F.WriteContentAsync(7, It.IsAny<GtfsFeed>(), It.IsAny<CancellationToken>()), Times.Once);
            m_Feeds.Verify(p_F => p_F.ActivateAsync(7, FeedSource.Network, "Lignes : +0 -1 (supprimés : L9) | Arrêts : +0 -0", It.IsAny<CancellationToken>()), Times.Once);
            m_Feeds.Verify(p_F => p_F.RebuildMatchesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Invalid_Feed_Marks_Version_Failed_And_Keeps_Active_Version()
        {
            m_Parser.Setup(p_P => p_P.Parse(It.IsAny<Stream>())).Throws(new GtfsFormatException("Fichier obligatoire stops.txt absent"));

            ValueResponse<FeedVersionInfo> v_Result = await RunAsync();

            Check.That(v_Result.Success).IsFalse();
            Check.That(v_Result.Errors.Single().Code).IsEqualTo(DomainErrors.Gtfs.InvalidFeed);
            m_Feeds.Verify(p_F => p_F.FailAsync(7, "Fichier obligatoire stops.txt absent", It.IsAny<CancellationToken>()), Times.Once);
            m_Feeds.Verify(p_F => p_F.ActivateAsync(It.IsAny<int>(), It.IsAny<FeedSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Download_Failure_Is_Journaled_As_Failed_Version()
        {
            m_Source.Setup(p_S => p_S.DownloadAsync(FeedSource.Network, It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("503"));

            ValueResponse<FeedVersionInfo> v_Result = await RunAsync();

            Check.That(v_Result.Errors.Single().Code).IsEqualTo(DomainErrors.Gtfs.DownloadFailed);
            m_Feeds.Verify(p_F => p_F.StartVersionAsync(FeedSource.Network, "https://exemple/gtfs.zip", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
            m_Feeds.Verify(p_F => p_F.FailAsync(7, It.Is<string>(p_M => p_M.Contains("503")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Write_Failure_Returns_Import_Failed_And_Purges_Version()
        {
            m_Parser.Setup(p_P => p_P.Parse(It.IsAny<Stream>())).Returns(GtfsFeedBuilder.Valid());
            m_Feeds.Setup(p_F => p_F.WriteContentAsync(7, It.IsAny<GtfsFeed>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("SQL"));

            ValueResponse<FeedVersionInfo> v_Result = await RunAsync();

            Check.That(v_Result.Errors.Single().Code).IsEqualTo(DomainErrors.Gtfs.ImportFailed);
            m_Feeds.Verify(p_F => p_F.FailAsync(7, "SQL", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
