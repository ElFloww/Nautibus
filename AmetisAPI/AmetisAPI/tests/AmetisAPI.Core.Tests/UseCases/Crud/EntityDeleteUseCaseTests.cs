using AmetisAPI.Core.Requests.Crud;
using AmetisAPI.Core.UseCases.Crud;
using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;

namespace AmetisAPI.Core.Tests.UseCases.Crud
{
    [Collection("UseCases")]
    public class EntityDeleteUseCaseTests
    {
        private readonly MockRepository m_MockRepository;

        private readonly Mock<IUnitOfWork> m_MockUnitOfWork;
        private readonly Mock<ILogger<EntityDeleteUseCase<IBoEntity>>> m_MockLogger;

        public EntityDeleteUseCaseTests()
        {
            m_MockRepository = new MockRepository(MockBehavior.Strict);

            m_MockUnitOfWork = m_MockRepository.Create<IUnitOfWork>();
            m_MockLogger = m_MockRepository.Create<ILogger<EntityDeleteUseCase<IBoEntity>>>();
        }

        private EntityDeleteUseCase<IBoEntity> CreateListUserUseCase()
        {
            return new EntityDeleteUseCase<IBoEntity>(m_MockUnitOfWork.Object, m_MockLogger.Object);
        }

        [Fact]
        public void Handle_ExpectedBehavior()
        {
            EntityDeleteUseCase<IBoEntity> v_EntityDeleteUseCase = CreateListUserUseCase();
            EntityDeleteRequest<IBoEntity> v_Request = new([1]);
            CancellationToken v_CancellationToken = default;

            m_MockUnitOfWork
                    .Setup(p_X => p_X.GetCrudRepository<IBoEntity>().DeleteEntityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(true));

            m_MockUnitOfWork.Setup(p_X => p_X.Save())
                .Returns(1);

            Check.ThatCode(() => v_EntityDeleteUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult().WhichMember(p_X => p_X.Success).Verifies(p_X => p_X.IsTrue());

            m_MockRepository.VerifyAll();
        }

        [Fact]
        public void Handle_UnexpectedBehavior()
        {
            EntityDeleteUseCase<IBoEntity> v_EntityDeleteUseCase = CreateListUserUseCase();
            EntityDeleteRequest<IBoEntity> v_Request = new([1]);
            CancellationToken v_CancellationToken = default;

            m_MockUnitOfWork
                .Setup(p_X => p_X.GetCrudRepository<IBoEntity>().DeleteEntityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            m_MockLogger.Setup(p_X =>
                    p_X.Log(It.Is<LogLevel>(p_Level => p_Level == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            ICheck<DeleteListResponse> v_Result = Check
                .ThatCode(() => v_EntityDeleteUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult();
            v_Result.WhichMember(p_X => p_X.Value.All(p_Y => p_Y.Value == false))
                .Verifies(p_X => p_X.IsTrue());
        }
    }
}