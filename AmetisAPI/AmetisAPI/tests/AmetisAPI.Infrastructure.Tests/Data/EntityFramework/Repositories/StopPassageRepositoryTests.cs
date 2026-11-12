using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Data.EntityFramework.Repositories;
using AmetisAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    [Collection(ISqlFixture.Collection)]
    public class StopPassageRepositoryTests(LocalDbFixture p_Fixture)
    {
        private static readonly DateTime m_Now = new(2026, 11, 12, 13, 0, 0, DateTimeKind.Utc);

        private AppDbContext NewContext() => DbContextFactory<AppDbContext>.InitContext(p_Fixture.ConnectionString);

        private static StopPassage At(string p_Trip, string p_Stop, int p_Order, DateTime p_Scheduled, int p_Delay) =>
            new(p_Trip, "11", p_Stop, p_Order, p_Scheduled, p_Scheduled.AddSeconds(p_Delay), p_Delay, "822");

        [Fact]
        public async Task Upsert_Updates_Without_Duplicates_And_Trip_Keeps_Only_The_Closest_Run()
        {
            await using AppDbContext v_Context = NewContext();
            await v_Context.Database.ExecuteSqlRawAsync("DELETE FROM rt.StopPassage");
            StopPassageRepository v_Repository = new(v_Context);

            await v_Repository.UpsertAsync([At("T", "A", 1, m_Now, 60), At("T", "B", 2, m_Now.AddMinutes(5), 60)], m_Now, CancellationToken.None);
            await v_Repository.UpsertAsync([At("T", "A", 1, m_Now, 90), At("T", "B", 2, m_Now.AddMinutes(5), 120)], m_Now.AddSeconds(30), CancellationToken.None);
            // Même course la veille : ne doit pas se mélanger à la circulation du jour.
            await v_Repository.UpsertAsync([At("T", "A", 1, m_Now.AddDays(-1), 0)], m_Now.AddDays(-1), CancellationToken.None);

            IReadOnlyList<StopPassage> v_Trip = await v_Repository.ListForTripAsync("T", m_Now.AddMinutes(2), CancellationToken.None);

            Check.That(await v_Context.StopPassages.CountAsync()).IsEqualTo(3);
            Check.That(v_Trip.Select(p_P => p_P.StopId)).ContainsExactly("A", "B");
            Check.That(v_Trip.Select(p_P => p_P.DelaySeconds)).ContainsExactly(90, 120);
            Check.That(v_Trip[0].ScheduledUtc.Kind).IsEqualTo(DateTimeKind.Utc);
        }

        [Fact]
        public async Task Stop_Passages_Flag_The_Terminus()
        {
            await using AppDbContext v_Context = NewContext();
            await v_Context.Database.ExecuteSqlRawAsync("DELETE FROM rt.StopPassage");
            StopPassageRepository v_Repository = new(v_Context);
            await v_Repository.UpsertAsync([At("T", "A", 1, m_Now, 0), At("T", "B", 2, m_Now.AddMinutes(5), 0), At("U", "B", 1, m_Now.AddMinutes(7), 0), At("U", "C", 2, m_Now.AddMinutes(9), 0)], m_Now, CancellationToken.None);

            IReadOnlyList<StopPassage> v_AtB = await v_Repository.ListForStopsAsync(["B"], m_Now, m_Now.AddHours(1), CancellationToken.None);

            Check.That(v_AtB.Single(p_P => p_P.TripId == "T").IsLastStop).IsTrue();
            Check.That(v_AtB.Single(p_P => p_P.TripId == "U").IsLastStop).IsFalse();
        }
    }
}
