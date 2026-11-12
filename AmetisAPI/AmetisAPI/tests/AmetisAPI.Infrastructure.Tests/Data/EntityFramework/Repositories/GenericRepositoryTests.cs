using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using AmetisAPI.Infrastructure.Tests.Fixtures;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    public class GenericRepositoryTests(LocalDbFixture p_SqlServerFixture) : ADatabaseTests(p_SqlServerFixture)
    {
        private TestRepository CreateTestRepository()
        {
            return new TestRepository(m_SqlServerFixture.AppDbContext, m_Mapper, null);
        }

        [Fact]
        public void UnexpectedConstructorBehavior()
        {
            Check.ThatCode(() => new TestRepository(null, null, null)).Throws<ArgumentNullException>();
            Check.ThatCode(() => new TestRepository(null, m_Mapper, null)).Throws<ArgumentNullException>();
            m_MockRepository.VerifyAll();
        }

        [Fact]
        public void AddUnexpected()
        {
            TestRepository v_TestRepository = CreateTestRepository();
            Check.ThatCode(async () => await v_TestRepository.Add(default(IEntity))).Throws<ArgumentNullException>();
        }

        [Fact]
        public void AddEnumerableUnexpected()
        {
            TestRepository v_TestRepository = CreateTestRepository();
            Check.ThatCode(async () => await v_TestRepository.Add(default(IEnumerable<IEntity>))).Throws<ArgumentNullException>();

            Check.ThatCode(async () => await v_TestRepository.Add([])).Throws<ArgumentNullException>();
        }

        [Fact]
        public void ExistsUnexpected()
        {
            TestRepository v_TestRepository = CreateTestRepository();
            Check.ThatCode(async () => await v_TestRepository.Exists(null)).Throws<ArgumentNullException>();
        }

        [Fact]
        public void GetUnexpected()
        {
            TestRepository v_TestRepository = CreateTestRepository();
            Check.ThatCode(async () => await v_TestRepository.Get(null)).Throws<ArgumentNullException>();
        }

        [Fact]
        public void UpdateUnexpected()
        {
            TestRepository v_TestRepository = CreateTestRepository();
            Check.ThatCode(async () => await v_TestRepository.Update(default(IEntity))).Throws<ArgumentNullException>();
        }

        [Fact]
        public void UpdateEnumerableUnexpected()
        {
            TestRepository v_TestRepository = CreateTestRepository();
            Check.ThatCode(async () => await v_TestRepository.Update(default(IEnumerable<IEntity>))).Throws<ArgumentNullException>();

            Check.ThatCode(async () => await v_TestRepository.Update([])).Throws<ArgumentNullException>();
        }

        [Fact]
        public void DisposeExpected()
        {
            Check.ThatCode(() =>
            {
                using TestRepository v_TestRepository = CreateTestRepository();
            }).DoesNotThrow();
        }
    }
}