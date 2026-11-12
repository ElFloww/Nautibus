using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Bo.Events;
using AmetisAPI.Domain.Bo.Push;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using AmetisAPI.Infrastructure.Data.EntityFramework.Repositories;
using AmetisAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    [Collection(ISqlFixture.Collection)]
    public class EventAndPushRepositoriesTests(LocalDbFixture p_Fixture)
    {
        private static readonly DateTime m_Now = new(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);

        private AppDbContext NewContext() => DbContextFactory<AppDbContext>.InitContext(p_Fixture.ConnectionString);

        private static NetworkEventDraft Detour(string p_Line) => new($"detour:{p_Line}", $"Déviation probable – ligne {p_Line}", "estimée", null, [p_Line], null, null, MatchConfidence.Low);

        private async Task ResetAsync()
        {
            await using AppDbContext v_Context = NewContext();
            await v_Context.Database.ExecuteSqlRawAsync("DELETE FROM rt.NetworkEvent; DELETE FROM app.Favorite; DELETE FROM app.PushSubscription; DELETE FROM AspNetUsers;");
        }

        [Fact]
        public async Task Detour_Is_Created_Once_Then_Closed_After_Grace_Period()
        {
            await ResetAsync();
            await using AppDbContext v_Context = NewContext();
            NetworkEventRepository v_Events = new(v_Context);
            TimeSpan v_Grace = TimeSpan.FromMinutes(10);

            Check.That(await v_Events.SyncAsync(NetworkEventKind.DetectedDetour, [Detour("N1")], m_Now, v_Grace, CancellationToken.None)).HasSize(1);
            Check.That(await v_Events.SyncAsync(NetworkEventKind.DetectedDetour, [Detour("N1")], m_Now.AddMinutes(1), v_Grace, CancellationToken.None)).IsEmpty();
            await v_Events.SyncAsync(NetworkEventKind.DetectedDetour, [], m_Now.AddMinutes(5), v_Grace, CancellationToken.None);
            Check.That((await v_Events.ListAsync(m_Now.AddDays(-1), CancellationToken.None)).Single().ClosedUtc).IsNull();

            await v_Events.SyncAsync(NetworkEventKind.DetectedDetour, [], m_Now.AddMinutes(12), v_Grace, CancellationToken.None);

            NetworkEvent v_Event = (await v_Events.ListAsync(m_Now.AddDays(-1), CancellationToken.None)).Single();
            Check.That(v_Event.ClosedUtc).IsEqualTo(m_Now.AddMinutes(12));
            Check.That(v_Event.LineIds).ContainsExactly("N1");
            Check.That(v_Event.IsActiveAt(m_Now.AddMinutes(13))).IsFalse();
        }

        [Fact]
        public async Task Official_Alert_Is_Closed_As_Soon_As_It_Leaves_The_Feed()
        {
            await ResetAsync();
            await using AppDbContext v_Context = NewContext();
            NetworkEventRepository v_Events = new(v_Context);
            NetworkEventDraft v_Alert = new("alert:1", "Travaux", null, null, ["N1"], m_Now, null, MatchConfidence.High);

            await v_Events.SyncAsync(NetworkEventKind.Official, [v_Alert], m_Now, TimeSpan.Zero, CancellationToken.None);
            await v_Events.SyncAsync(NetworkEventKind.Official, [], m_Now.AddSeconds(30), TimeSpan.Zero, CancellationToken.None);

            Check.That((await v_Events.ListAsync(m_Now.AddDays(-1), CancellationToken.None)).Single().ClosedUtc).IsEqualTo(m_Now.AddSeconds(30));
        }

        [Fact]
        public async Task Subscribers_Are_Selected_By_All_Lines_Or_Favorite_Line()
        {
            await ResetAsync();
            await using AppDbContext v_Context = NewContext();
            v_Context.Users.AddRange(new AppUser { Id = "tout", UserName = "tout" }, new AppUser { Id = "fan", UserName = "fan" }, new AppUser { Id = "autre", UserName = "autre" });
            await v_Context.SaveChangesAsync();
            FavoriteRepository v_Favorites = new(v_Context);
            await v_Favorites.AddAsync("fan", FavoriteKind.Line, "N1", m_Now, CancellationToken.None);
            await v_Favorites.AddAsync("autre", FavoriteKind.Line, "N2", m_Now, CancellationToken.None);

            PushSubscriptionRepository v_Subscriptions = new(v_Context);
            await v_Subscriptions.SaveAsync("tout", new PushSubscriptionInfo("https://fcm.googleapis.com/1", "k", "a"), true, m_Now, CancellationToken.None);
            await v_Subscriptions.SaveAsync("fan", new PushSubscriptionInfo("https://fcm.googleapis.com/2", "k", "a"), false, m_Now, CancellationToken.None);
            await v_Subscriptions.SaveAsync("autre", new PushSubscriptionInfo("https://fcm.googleapis.com/3", "k", "a"), false, m_Now, CancellationToken.None);
            // Même navigateur réabonné : mis à jour, pas dupliqué.
            await v_Subscriptions.SaveAsync("fan", new PushSubscriptionInfo("https://fcm.googleapis.com/2", "k2", "a2"), false, m_Now, CancellationToken.None);

            IReadOnlyList<PushSubscriptionInfo> v_Targets = await v_Subscriptions.ListForLinesAsync(["N1"], CancellationToken.None);

            Check.That(v_Targets.Select(p_T => p_T.Endpoint).OrderBy(p_E => p_E)).ContainsExactly("https://fcm.googleapis.com/1", "https://fcm.googleapis.com/2");
            Check.That(v_Targets.Single(p_T => p_T.Endpoint.EndsWith("/2")).P256dh).IsEqualTo("k2");
            Check.That(await v_Subscriptions.RemoveAsync("autre", "https://fcm.googleapis.com/2", CancellationToken.None)).IsFalse();
            Check.That(await v_Context.PushSubscriptions.CountAsync()).IsEqualTo(3);
        }
    }
}
