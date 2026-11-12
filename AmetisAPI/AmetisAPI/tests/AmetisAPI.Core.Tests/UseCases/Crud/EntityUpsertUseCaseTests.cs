using AmetisAPI.Core.Requests.Crud;
using AmetisAPI.Core.UseCases.Crud;
using AmetisAPI.Domain.Errors;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using AmetisAPI.Domain.Tests.Bo;

namespace AmetisAPI.Core.Tests.UseCases.Crud
{
    [Collection("UseCases")]
    public class EntityUpsertUseCaseTests
    {
        private readonly MockRepository m_MockRepository;

        private readonly Mock<IUnitOfWork> m_MockUnitOfWork;
        private readonly Mock<ILogger<EntityUpsertUseCase<DummyClass>>> m_MockLoggerUser;
        private readonly Mock<IMappingAddEntity<DummyClass, IEntity>> m_MockUserMap;

        public EntityUpsertUseCaseTests()
        {
            m_MockRepository = new MockRepository(MockBehavior.Strict);

            m_MockUnitOfWork = m_MockRepository.Create<IUnitOfWork>();
            m_MockLoggerUser = m_MockRepository.Create<ILogger<EntityUpsertUseCase<DummyClass>>>();
            m_MockUserMap = m_MockRepository.Create<IMappingAddEntity<DummyClass, IEntity>>();
        }

        private EntityUpsertUseCase<DummyClass> CreateUpsertUserUseCase()
        {
            return new EntityUpsertUseCase<DummyClass>(m_MockUnitOfWork.Object, m_MockLoggerUser.Object);
        }

        [Fact]
        public void Handle_AddEntity_ExpectedBehavior()
        {
            DummyClass v_NewDummy = new()
            {
                Id = null,
                Name = "Name"
            };

            EntityUpsertUseCase<DummyClass> v_UpsertUserUseCase = CreateUpsertUserUseCase();
            EntityUpsertRequest<DummyClass> v_Request = new(v_NewDummy);
            CancellationToken v_CancellationToken = default;

            m_MockUserMap.Setup(p_X => p_X.MapBoEntity).Returns(new DummyClass { Id = 5 });

            m_MockUnitOfWork
                .Setup(p_X => p_X.GetCrudRepository<DummyClass>().AddEntityAsync(It.IsAny<DummyClass>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(m_MockUserMap.Object);

            m_MockUnitOfWork.Setup(p_X => p_X.Save())
                .Returns(1);

            Check.ThatCode(() => v_UpsertUserUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult().WhichMember(p_X => p_X.Success).Verifies(p_X => p_X.IsTrue());

            m_MockRepository.VerifyAll();
        }

        [Fact]
        public void Handle_UpdateDummy_ExpectedBehavior()
        {
            DummyClass v_NewDummy = new()
            {
                Id = 1,
                Name = "Name"
            };

            var v_UpsertUserUseCase = CreateUpsertUserUseCase();
            EntityUpsertRequest<DummyClass> v_Request = new(v_NewDummy);
            CancellationToken v_CancellationToken = default;

            m_MockUnitOfWork
                .Setup(p_X => p_X.GetCrudRepository<DummyClass>().UpdateEntityAsync(It.IsAny<DummyClass>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            m_MockUnitOfWork.Setup(p_X => p_X.Save())
                .Returns(1);

            Check.ThatCode(() => v_UpsertUserUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult().WhichMember(p_X => p_X.Success).Verifies(p_X => p_X.IsTrue());

            m_MockRepository.VerifyAll();
        }

        [Fact]
        public void Handle_UnexpectedBehavior()
        {
            DummyClass v_NewDummy = new()
            {
                Id = 1,
                Name = "Name"
            };

            var v_UpsertUserUseCase = CreateUpsertUserUseCase();
            EntityUpsertRequest<DummyClass> v_Request = new(v_NewDummy);
            CancellationToken v_CancellationToken = default;

            m_MockUnitOfWork
                .Setup(p_X => p_X.GetCrudRepository<DummyClass>().AddEntityAsync(It.IsAny<DummyClass>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            m_MockLoggerUser.Setup(p_X =>
                   p_X.Log(It.Is<LogLevel>(p_Level => p_Level == LogLevel.Error),
                       It.IsAny<EventId>(),
                       It.IsAny<It.IsAnyType>(),
                       It.IsAny<Exception>(),
                       It.IsAny<Func<It.IsAnyType, Exception, string>>()))
               .Verifiable();

            var v_Result = Check.ThatCode(() => v_UpsertUserUseCase.Handle(v_Request, v_CancellationToken))
                .DoesNotThrow().And
                .IsNotNull().And
                .WhichResult();
            v_Result.WhichMember(p_X => p_X.Success).Verifies(p_X => p_X.IsFalse());
            v_Result.WhichMember(p_X => p_X.Errors.Select(p_E => p_E.Code)).Verifies(p_X => p_X.Contains(DomainErrors.UnknowError));
        }
    }
}