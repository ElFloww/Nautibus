using AmetisAPI.Infrastructure.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework
{
    public class UnitOfWorkTests
    {
        private UnitOfWork<DbContext> m_UnitOfWork;
        private readonly Mock<DbContext> m_DbContextMock;
        private readonly Mock<IMapper> m_MapperMock;
        private readonly Mock<ILoggerFactory> m_LoggerFactoryMock;
        private readonly Mock<IComponentContext> m_ComponentContextMock;

        public UnitOfWorkTests()
        {
            m_DbContextMock = new Mock<DbContext>();
            Mock<DatabaseFacade> v_DatabaseMock = new(m_DbContextMock.Object);
            m_DbContextMock.SetupGet(p_M => p_M.Database).Returns(v_DatabaseMock.Object);
            m_MapperMock = new Mock<IMapper>();
            m_LoggerFactoryMock = new Mock<ILoggerFactory>();
            m_ComponentContextMock = new Mock<IComponentContext>();
        }

        [Fact]
        public void Constructor_Should_Throw_ArgumentNullException_When_DbContext_Is_Null()
        {
            // Arrange
            DbContext v_DbContext = null;

            // Act & Assert
            Check.ThatCode(() => new UnitOfWork<DbContext>(v_DbContext, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object)).Throws<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_Should_Throw_ArgumentNullException_When_Mapper_Is_Null()
        {
            // Arrange
            IMapper v_Mapper = null;

            // Act & Assert
            Check.ThatCode(() => new UnitOfWork<DbContext>(m_DbContextMock.Object, v_Mapper, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object)).Throws<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_Should_Throw_ArgumentNullException_When_LoggerFactory_Is_Null()
        {
            // Arrange
            ILoggerFactory v_LoggerFactory = null;

            // Act & Assert
            Check.ThatCode(() => new UnitOfWork<DbContext>(m_DbContextMock.Object, m_MapperMock.Object, v_LoggerFactory, m_ComponentContextMock.Object)).Throws<ArgumentNullException>();
        }

        [Fact]
        public void Save_Should_Call_SaveChanges_Method_Of_DbContext()
        {
            // Arrange
            UnitOfWork<DbContext> v_UnitOfWork = new(m_DbContextMock.Object, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object);

            // Act
            _ = v_UnitOfWork.Save();

            // Assert
            m_DbContextMock.Verify(p_M => p_M.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Save_Should_Return_Number_Of_Changes_Saved()
        {
            // Arrange
            UnitOfWork<DbContext> v_Uow = new(m_DbContextMock.Object, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object);
            m_DbContextMock.Setup(p_D => p_D.SaveChanges()).Returns(2);

            // Act
            int v_Result = v_Uow.Save();

            // Assert
            Check.That(v_Result).IsEqualTo(2);
        }

        [Fact]
        public void Save_WhenCalled_ShouldSaveChangesInDbContext()
        {
            m_UnitOfWork = new UnitOfWork<DbContext>(m_DbContextMock.Object, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object);

            // Act
            _ = m_UnitOfWork.Save();

            // Assert
            m_DbContextMock.Verify(p_M => p_M.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Dispose_ShouldDisposeDbContext()
        {
            m_UnitOfWork = new UnitOfWork<DbContext>(m_DbContextMock.Object, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object);
            m_DbContextMock.Setup(p_X => p_X.Dispose()).Verifiable();

            // Act
            m_UnitOfWork.Dispose();

            // Assert
            m_DbContextMock.Verify(p_M => p_M.Dispose(), Times.Once);
        }

        [Fact]
        public void BeginTransaction_WhenCalled_ShouldBeginTransactionInDbContext()
        {
            m_UnitOfWork = new UnitOfWork<DbContext>(m_DbContextMock.Object, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object);

            // Act
            m_UnitOfWork.BeginTransaction();

            // Assert
            m_DbContextMock.Verify(p_M => p_M.Database.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void CommitTransaction_WhenCalled_ShouldCommitTransactionInDbContext()
        {
            m_UnitOfWork = new UnitOfWork<DbContext>(m_DbContextMock.Object, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object);

            // Act
            m_UnitOfWork.CommitTransaction();

            // Assert
            m_DbContextMock.Verify(p_M => p_M.Database.CommitTransaction(), Times.Once);
        }

        [Fact]
        public void RollbackTransaction_WhenCalled_ShouldRollbackTransactionInDbContext()
        {
            m_UnitOfWork = new UnitOfWork<DbContext>(m_DbContextMock.Object, m_MapperMock.Object, m_LoggerFactoryMock.Object, m_ComponentContextMock.Object);

            // Act
            m_UnitOfWork.RollbackTransaction();

            // Assert
            m_DbContextMock.Verify(p_M => p_M.Database.RollbackTransaction(), Times.Once);
        }
    }
}