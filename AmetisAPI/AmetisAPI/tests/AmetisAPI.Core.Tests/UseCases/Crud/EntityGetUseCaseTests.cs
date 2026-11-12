using AmetisAPI.Core.Requests.Crud;
using AmetisAPI.Core.Responses.Crud;
using AmetisAPI.Core.UseCases.Crud;
using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Tests.Bo;

namespace AmetisAPI.Core.Tests.UseCases.Crud
{
    [Collection("UseCases")]
    public class EntityGetUseCaseTests
    {
        private readonly MockRepository m_MockRepository;

        private readonly Mock<IUnitOfWork> m_MockUnitOfWork;
        private readonly Mock<ILogger<EntityGetUseCase<IBoEntity>>> m_MockLogger;

        public EntityGetUseCaseTests()
        {
            m_MockRepository = new MockRepository(MockBehavior.Strict);

            m_MockUnitOfWork = m_MockRepository.Create<IUnitOfWork>();
            m_MockLogger = m_MockRepository.Create<ILogger<EntityGetUseCase<IBoEntity>>>();
        }

        private EntityGetUseCase<IBoEntity> CreateListUserUseCase()
        {
            return new EntityGetUseCase<IBoEntity>(m_MockUnitOfWork.Object, m_MockLogger.Object);
        }

        [Fact]
        public void Handle_ExpectedBehavior()
        {
            EntityGetUseCase<IBoEntity> v_GetListUserUseCase = CreateListUserUseCase();
            EntityGetRequest<IBoEntity> v_Request = new(1);
            CancellationToken v_CancellationToken = default;

            m_MockUnitOfWork
                .Setup(p_X => p_X.GetCrudRepository<IBoEntity>().GetEntityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new DummyClass()
                );

            Check.ThatCode(() => v_GetListUserUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult().WhichMember(p_X => p_X.Success).Verifies(p_X => p_X.IsTrue());

            m_MockRepository.VerifyAll();
        }

        [Fact]
        public void Handle_Null_ExpectedBehavior()
        {
            EntityGetUseCase<IBoEntity> v_GetListUserUseCase = CreateListUserUseCase();
            EntityGetRequest<IBoEntity> v_Request = new(1);
            CancellationToken v_CancellationToken = default;

            m_MockUnitOfWork
                .Setup(p_X => p_X.GetCrudRepository<IBoEntity>().GetEntityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    null as IBoEntity
                );

            ICheck<EntityResponse<IBoEntity>> v_Result = Check
                .ThatCode(() => v_GetListUserUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult();
            v_Result.WhichMember(p_X => p_X.Success)
                .Verifies(p_X => p_X.IsFalse());
            v_Result.WhichMember(p_X => p_X.Errors.Select(p_E => p_E.Code))
                .Verifies(p_X => p_X.Contains(DomainErrors.Data.NotFound));

            m_MockRepository.VerifyAll();
        }

        [Fact]
        public void Handle_UnexpectedBehavior()
        {
            EntityGetUseCase<IBoEntity> v_GetListUserUseCase = CreateListUserUseCase();
            EntityGetRequest<IBoEntity> v_Request = new(1);
            CancellationToken v_CancellationToken = default;

            m_MockUnitOfWork
                .Setup(p_X => p_X.GetCrudRepository<IBoEntity>().GetEntityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            m_MockLogger.Setup(p_X =>
                    p_X.Log(It.Is<LogLevel>(p_Level => p_Level == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            ICheck<EntityResponse<IBoEntity>> v_Result = Check
                .ThatCode(() => v_GetListUserUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult();
            v_Result.WhichMember(p_X => p_X.Success)
                .Verifies(p_X => p_X.IsFalse());
            v_Result.WhichMember(p_X => p_X.Errors.Select(p_E => p_E.Code))
                .Verifies(p_X => p_X.Contains(DomainErrors.UnknowError));
        }
    }
}