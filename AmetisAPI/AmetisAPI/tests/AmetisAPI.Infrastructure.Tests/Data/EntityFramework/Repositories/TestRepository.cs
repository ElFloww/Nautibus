using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using AmetisAPI.Infrastructure.Data.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    internal class TestRepository(DbContext p_DbContext, IMapper p_Mapper, ILogger p_Logger)
        : GenericRepository<IEntity>(p_DbContext, p_Mapper, p_Logger);
}