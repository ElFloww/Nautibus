using AmetisAPI.Infrastructure.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework
{
    public class TestContext(DbContextOptions<TestContext> p_Options) : BaseAppDbContext(p_Options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}