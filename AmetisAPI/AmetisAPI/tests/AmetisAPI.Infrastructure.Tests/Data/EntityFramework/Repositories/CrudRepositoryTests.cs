using AmetisAPI.Domain.Bo.Crud;
using AmetisAPI.Domain.Interfaces.Data.EntityFramework.Entities;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Data.EntityFramework.Repositories;
using AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Entities;
using AmetisAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    public class CrudRepositoryTests : ADatabaseTests
    {
        private readonly ILoggerFactory m_LoggerFactory;

        public CrudRepositoryTests(LocalDbFixture p_SqlServerFixture) : base(p_SqlServerFixture)
        {
            m_LoggerFactory = NullLoggerFactory.Instance;
        }

        private object CreateEntitiesRepository(AppDbContext p_Ctx, Type p_BoType, Type p_EntityType, ILogger p_GoodLogger)
        {
            Type v_Type = typeof(CrudRepository<,>);
            Type v_GenericType = v_Type.MakeGenericType(p_BoType, p_EntityType);
            return Activator.CreateInstance(
                v_GenericType,
                p_Ctx,
                m_Mapper,
               p_GoodLogger
            );
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory(Skip = "Skipped until a IIDBOEntity is created")]
        [MemberData(nameof(DtoExpectedBehaviorData))]
        public async Task ListEntity_ExpectedBehavior_Async(Type p_TypeBo, Type p_TypeEntity, InsertFunc p_InsertFunc, ILogger p_Logger, UpsertFunc p_UpsertEntity, PropToCheck p_PropToCheck, AddEntityFunc p_AddEntity, DeleteEntityFunc p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;
            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);
                await v_Ctx.SaveChangesAsync();

                Check.ThatCode(async () =>
                {
                    IEnumerable<IBoEntity> v_Result = await v_Repository.ListEntitiesAsync();
                    return v_Result;
                })
                .DoesNotThrow()
                .And.IsNotNull();
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory(Skip = "Waiting for an Dto to be created")]
        [MemberData(nameof(DtoExpectedBehaviorData))]
        public async Task AddEntity_ExpectedBehavior_Async(Type p_TypeBo, Type p_TypeEntity, InsertFunc p_InsertFunc, ILogger p_Logger, UpsertFunc p_UpsertEntity, PropToCheck p_PropToCheck, AddEntityFunc p_AddEntity, DeleteEntityFunc p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;
            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);
                await v_Repository.AddEntityAsync((dynamic)p_AddEntity.Invoke(v_Ctx));
                await v_Ctx.SaveChangesAsync();

                IEnumerable<IIdBoEntity> v_Result = await v_Repository.ListEntitiesAsync();

                Check.That(v_Result).IsNotNull().And.CountIs(1);
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory(Skip = "Waiting for an Dto to be created")]
        [MemberData(nameof(DtoExpectedBehaviorData))]
        public async Task UpsertEntity_ExpectedBehavior_Async(Type p_TypeBo, Type p_TypeEntity, InsertFunc p_InsertFunc, ILogger p_Logger, UpsertFunc p_UpsertEntity, PropToCheck p_PropToCheck, AddEntityFunc p_AddEntity, DeleteEntityFunc p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;
            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);
                IIdEntity v_EntityWithId = p_InsertFunc.Invoke(v_Ctx).Entity as IIdEntity;
                await v_Ctx.SaveChangesAsync();

                IEnumerable<IIdBoEntity> v_Entities = await v_Repository.ListEntitiesAsync();

                IIdBoEntity v_D1 = v_Entities.First();
                string v_NewValue = "DPT2";
                v_D1 = p_UpsertEntity(v_D1, v_Ctx, v_NewValue);
                await v_Repository.UpdateEntityAsync((dynamic)v_D1, default(CancellationToken));
                await v_Ctx.SaveChangesAsync();

                v_Entities = await v_Repository.ListEntitiesAsync();
                IIdBoEntity v_D2 = v_Entities.First();
                Check.That(p_PropToCheck(v_D2).ToString()).IsEqualTo(v_NewValue);
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory(Skip = "Waiting for an Dto to be created")]
        [MemberData(nameof(DtoExpectedBehaviorData))]
        public async Task GetEntity_ExpectedBehavior_Async(Type p_TypeBo, Type p_TypeEntity, InsertFunc p_InsertFunc, ILogger p_Logger, UpsertFunc p_UpsertEntity, PropToCheck p_PropToCheck, AddEntityFunc p_AddEntity, DeleteEntityFunc p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;
            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);

                IIdEntity v_EntityWithId = p_InsertFunc.Invoke(v_Ctx).Entity as IIdEntity;
                await v_Ctx.SaveChangesAsync();

                Check.ThatCode(async () =>
                {
                    IBoEntity v_Entity = await v_Repository.GetEntityAsync(v_EntityWithId.Id, default(CancellationToken));
                    return v_Entity;
                })
                .DoesNotThrow()
                .And.IsNotNull();
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory(Skip = "Waiting for an Dto to be created")]
        [MemberData(nameof(DtoExpectedBehaviorData))]
        public async Task DeleteEntity_ExpectedBehavior_Async(Type p_TypeBo, Type p_TypeEntity, InsertFunc p_InsertFunc, ILogger p_Logger, UpsertFunc p_UpsertEntity, PropToCheck p_PropToCheck, AddEntityFunc p_AddEntity, DeleteEntityFunc p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;
            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);

                IIdEntity v_EntityWithId = p_InsertFunc.Invoke(v_Ctx).Entity as IIdEntity;
                await v_Ctx.SaveChangesAsync();

                Check.ThatCode(async () =>
                {
                    return await v_Repository.DeleteEntityAsync(v_EntityWithId.Id, default(CancellationToken));
                })
                .DoesNotThrow()
                .And.IsNotNull();
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory]
        [MemberData(nameof(DtoUnexpectedBehaviorData))]
        public void AddEntity_UnexpectedBehavior(Type p_TypeBo, Type p_TypeEntity, ILogger p_Logger, AddEntityFuncUnexpected p_AddEntity, DeleteEntityFuncUnexpected p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;

            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);

                Check.ThatCode(async () =>
                {
                    await v_Repository.AddEntityAsync((dynamic)p_AddEntity.Invoke(v_Ctx));
                    await v_Ctx.SaveChangesAsync();
                }).Throws<ArgumentException>();
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory]
        [MemberData(nameof(DtoUnexpectedBehaviorData))]
        public void GetEntity_UnexpectedBehavior_Async(Type p_TypeBo, Type p_TypeEntity, ILogger p_Logger, AddEntityFuncUnexpected p_AddEntity, DeleteEntityFuncUnexpected p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;
            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);

                Check.ThatCode(async () =>
                {
                    await v_Repository.GetEntityAsync(1, default(CancellationToken));
                    await v_Ctx.SaveChangesAsync();
                }).Throws<ArgumentException>();
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

#pragma warning disable xUnit1026, xUnit1045, xUnit1044  // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc

        [Theory]
        [MemberData(nameof(DtoUnexpectedBehaviorData))]
        public void DeleteEntity_UnexpectedBehavior_Async(Type p_TypeBo, Type p_TypeEntity, ILogger p_Logger, AddEntityFuncUnexpected p_AddEntity, DeleteEntityFuncUnexpected p_DeleteEntity)
#pragma warning restore xUnit1026, xUnit1045, xUnit1044 // Theory methods should use all of their parameters, ILogger is not Serializable, as InsertFunc
        {
            AppDbContext v_Ctx = m_SqlServerFixture.AppDbContext;
            try
            {
                dynamic v_Repository = CreateEntitiesRepository(v_Ctx, p_TypeBo, p_TypeEntity, p_Logger);

                Check.ThatCode(async () =>
                {
                    await v_Repository.DeleteEntityAsync(1, default(CancellationToken));
                    await v_Ctx.SaveChangesAsync();
                }).Throws<ArgumentException>();
            }
            finally
            {
                ClearData(v_Ctx);
            }
        }

        public delegate EntityEntry InsertFunc(AppDbContext ctx);

        public delegate IIdBoEntity UpsertFunc(IIdBoEntity entity, AppDbContext ctx, string newValue);

        public delegate object PropToCheck(IIdBoEntity entity);

        public delegate IIdBoEntity AddEntityFunc(AppDbContext ctx);

        public delegate IIdBoEntity DeleteEntityFunc(AppDbContext ctx);

        public static TheoryData<
            Type,
            Type,
            InsertFunc,
            ILogger,
            UpsertFunc,
            PropToCheck,
            AddEntityFunc,
            DeleteEntityFunc
            > DtoExpectedBehaviorData()
        {
            TheoryData<Type, Type, InsertFunc, ILogger, UpsertFunc, PropToCheck, AddEntityFunc, DeleteEntityFunc> v_Data = new TheoryData<Type, Type, InsertFunc, ILogger, UpsertFunc, PropToCheck, AddEntityFunc, DeleteEntityFunc>();

            //Data block for every entity

            return v_Data;
        }

        public delegate IBoEntity AddEntityFuncUnexpected(AppDbContext ctx);

        public delegate IBoEntity DeleteEntityFuncUnexpected(AppDbContext ctx);

        public static TheoryData<
            Type,
            Type,
            ILogger,
            AddEntityFuncUnexpected,
            DeleteEntityFuncUnexpected
            > DtoUnexpectedBehaviorData()
        {
            TheoryData<Type, Type, ILogger, AddEntityFuncUnexpected, DeleteEntityFuncUnexpected> v_Data = new TheoryData<Type, Type, ILogger, AddEntityFuncUnexpected, DeleteEntityFuncUnexpected>();

            //Data block for every entity
            v_Data.Add(
                    //BO Type
                    typeof(FakeIBoEntity),

                    //Entity Type
                    typeof(FakeIIdEntity),

                    //Logger
                    Mock.Of<ILogger<CrudRepository<FakeIBoEntity, FakeIIdEntity>>>(),

                    //Add Entity Func
                    Ctx => new FakeIBoEntity(),

                    //Delete Entity Func
                    Ctx => new FakeIBoEntity()

                );
            return v_Data;
        }
    }
}