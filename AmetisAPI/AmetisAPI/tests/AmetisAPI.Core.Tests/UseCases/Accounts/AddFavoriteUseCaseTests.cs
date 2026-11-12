using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Core.UseCases.Accounts;
using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Accounts;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Network;

namespace AmetisAPI.Core.Tests.UseCases.Accounts
{
    public class AddFavoriteUseCaseTests
    {
        private readonly Mock<IFavoriteRepository> m_Favorites = new();
        private readonly Mock<INetworkRepository> m_Network = new();

        private Task<ValueResponse<bool>> RunAsync(FavoriteKind p_Kind, string p_Target) =>
            new AddFavoriteUseCase(Mock.Of<ILogger<AddFavoriteUseCase>>(), Mock.Of<IUnitOfWork>(), m_Favorites.Object, m_Network.Object, Mock.Of<IClock>())
                .Handle(new AddFavoriteRequest("user-1", p_Kind, p_Target), CancellationToken.None);

        [Fact]
        public async Task Unknown_Target_Is_Not_Found_And_Nothing_Stored()
        {
            ValueResponse<bool> v_Result = await RunAsync(FavoriteKind.Stop, "INCONNU");

            Check.That(v_Result.Errors.Single().Code).IsEqualTo(DomainErrors.Data.NotFound);
            m_Favorites.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Existing_Line_Is_Added_For_The_User()
        {
            m_Network.Setup(p_N => p_N.GetLineAsync("L1", It.IsAny<CancellationToken>())).ReturnsAsync(new LineDetail(new LineSummary("L1", "1", null, null, null, 1), []));
            m_Favorites.Setup(p_F => p_F.AddAsync("user-1", FavoriteKind.Line, "L1", It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            ValueResponse<bool> v_Result = await RunAsync(FavoriteKind.Line, "L1");

            Check.That(v_Result.Value).IsTrue();
        }
    }
}
