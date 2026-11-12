using AmetisAPI.Domain.Bo.Accounts;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Bo.Network;
using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Infrastructure.Data.EntityFramework;
using AmetisAPI.Infrastructure.Data.EntityFramework.Repositories;
using AmetisAPI.Infrastructure.Gtfs;
using AmetisAPI.Infrastructure.Tests.Fixtures;
using AmetisAPI.Infrastructure.Tests.Gtfs;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Tests.Data.EntityFramework.Repositories
{
    /// <summary>Tests d'intégration sur LocalDb (schéma déployé depuis le DACPAC).</summary>
    [Collection(ISqlFixture.Collection)]
    public class GtfsRepositoriesTests(LocalDbFixture p_Fixture)
    {
        private AppDbContext NewContext() => DbContextFactory<AppDbContext>.InitContext(p_Fixture.ConnectionString);

        private static GtfsFeed Feed() => new GtfsZipParser().Parse(GtfsZipFixture.Zip(GtfsZipFixture.Files()));

        private async Task ResetAsync()
        {
            await using AppDbContext v_Context = NewContext();
            await v_Context.Database.ExecuteSqlRawAsync(@"DELETE FROM gtfs.StopTime; DELETE FROM gtfs.StopRoute; DELETE FROM gtfs.Trip; DELETE FROM gtfs.Stop;
DELETE FROM gtfs.Route; DELETE FROM gtfs.Calendar; DELETE FROM gtfs.CalendarDate; DELETE FROM gtfs.ShapePoint; DELETE FROM gtfs.ImportRejection;
DELETE FROM gtfs.StopMatch; DELETE FROM gtfs.RouteMatch; DELETE FROM gtfs.FeedVersion; DELETE FROM rt.VehiclePosition; DELETE FROM app.Favorite; DELETE FROM AspNetUsers;");
        }

        private async Task<int> ImportAsync(FeedSource p_Source, GtfsFeed p_Feed, string p_Hash)
        {
            await using AppDbContext v_Context = NewContext();
            FeedRepository v_Repository = new(v_Context);
            int v_Id = await v_Repository.StartVersionAsync(p_Source, "https://exemple", p_Hash, CancellationToken.None);
            await v_Repository.WriteContentAsync(v_Id, p_Feed, CancellationToken.None);
            await v_Repository.ActivateAsync(v_Id, p_Source, "test", CancellationToken.None);
            return v_Id;
        }

        [Fact]
        public async Task Reimport_Replaces_Content_Without_Duplicates_And_Keeps_Journal()
        {
            await ResetAsync();
            int v_First = await ImportAsync(FeedSource.Network, Feed(), "H1");
            int v_Second = await ImportAsync(FeedSource.Network, Feed(), "H2");

            await using AppDbContext v_Context = NewContext();
            Check.That(await v_Context.StopTimes.CountAsync()).IsEqualTo(2);
            Check.That(await v_Context.Stops.Select(p_S => p_S.FeedVersionId).Distinct().ToListAsync()).ContainsExactly(v_Second);
            Check.That(await v_Context.FeedVersions.Where(p_V => p_V.IsActive).Select(p_V => p_V.Id).ToListAsync()).ContainsExactly(v_Second);
            Check.That(await v_Context.FeedVersions.CountAsync(p_V => p_V.Id == v_First)).IsEqualTo(1);
            Check.That(await v_Context.ImportRejections.CountAsync(p_R => p_R.FeedVersionId == v_Second)).IsEqualTo(2);
        }

        [Fact]
        public async Task Failed_Import_Keeps_Active_Version_And_Removes_Partial_Rows()
        {
            await ResetAsync();
            int v_Active = await ImportAsync(FeedSource.Network, Feed(), "H1");

            await using AppDbContext v_Context = NewContext();
            FeedRepository v_Repository = new(v_Context);
            int v_Broken = await v_Repository.StartVersionAsync(FeedSource.Network, "https://exemple", "H2", CancellationToken.None);
            await v_Repository.WriteContentAsync(v_Broken, Feed(), CancellationToken.None);
            await v_Repository.FailAsync(v_Broken, "panne simulée", CancellationToken.None);

            FeedVersionInfo v_Info = await v_Repository.GetAsync(v_Broken, CancellationToken.None);
            Check.That(v_Info.Status).IsEqualTo(FeedImportStatus.Failed);
            Check.That(v_Info.ErrorMessage).IsEqualTo("panne simulée");
            Check.That((await v_Repository.GetActiveAsync(FeedSource.Network, CancellationToken.None)).Id).IsEqualTo(v_Active);
            Check.That(await v_Context.Stops.CountAsync(p_S => p_S.FeedVersionId == v_Broken)).IsEqualTo(0);
            Check.That(v_Info.StartedAtUtc.Kind).IsEqualTo(DateTimeKind.Utc);
        }

        [Fact]
        public async Task Network_Queries_Read_Active_Version()
        {
            await ResetAsync();
            await ImportAsync(FeedSource.Network, Feed(), "H1");

            await using AppDbContext v_Context = NewContext();
            NetworkRepository v_Network = new(v_Context);

            StopDetail v_Stop = await v_Network.GetStopAsync("Q1", CancellationToken.None);
            Check.That(v_Stop.Stop.Id).IsEqualTo("AREA");
            Check.That(v_Stop.Stop.Lines.Single().ShortName).IsEqualTo("1");
            Check.That(v_Stop.Quays.Select(p_Q => p_Q.Id)).ContainsExactly("Q1");

            IReadOnlyList<StopSummary> v_Near = await v_Network.SearchStopsAsync(null, 49.8701, 2.2501, 100, 10, CancellationToken.None);
            Check.That(v_Near.Select(p_S => p_S.Id)).ContainsExactly("Q2");

            // Q2 a pickup_type = 1 (descente seulement) : pas de départ proposé.
            IReadOnlyList<ScheduledStopTime> v_Times = await v_Network.GetStopTimesAsync(["Q1", "Q2"], ["SEM"], 0, 200_000, CancellationToken.None);
            Check.That(v_Times.Single().StopId).IsEqualTo("Q1");
            Check.That(v_Times.Single().Destination).IsEqualTo("CHU");

            LineDetail v_Line = await v_Network.GetLineAsync("L1", CancellationToken.None);
            Check.That(v_Line.Directions.Single().Stops.Select(p_S => p_S.Id)).ContainsExactly("AREA", "Q2");
            Check.That((await v_Network.GetLineShapesAsync("L1", CancellationToken.None)).Single().Coordinates).HasSize(2);
        }

        [Fact]
        public async Task Realtime_Stops_And_Lines_Are_Matched_With_Confidence()
        {
            await ResetAsync();
            await ImportAsync(FeedSource.Network, Feed(), "H1");
            GtfsFeed v_Realtime = Feed();
            v_Realtime.Routes[0] = v_Realtime.Routes[0] with { RouteId = "RT1" };
            v_Realtime.Trips[0] = v_Realtime.Trips[0] with { RouteId = "RT1" };
            v_Realtime.Stops.RemoveAt(0);
            v_Realtime.Stops[0] = v_Realtime.Stops[0] with { StopId = "905", ParentStationId = null, Latitude = 49.89012 };
            v_Realtime.Stops[1] = v_Realtime.Stops[1] with { StopId = "906", Name = "Autre nom", Latitude = 49.8703 };
            v_Realtime.StopTimes.Clear();
            v_Realtime.StopTimes.Add(new GtfsStopTime("T1", 1, "905", 0, 0, 0, 0, null));
            await ImportAsync(FeedSource.Realtime, v_Realtime, "R1");

            await using AppDbContext v_Context = NewContext();
            FeedRepository v_Repository = new(v_Context);
            Check.That(await v_Repository.RebuildMatchesAsync(CancellationToken.None)).IsEqualTo(3);

            RealtimeReferenceRepository v_References = new(v_Context);
            IReadOnlyList<RealtimeStopMatch> v_Matches = await v_References.GetStopMatchesForNetworkStopsAsync(["Q1", "Q2"], CancellationToken.None);
            Check.That(v_Matches.Single(p_M => p_M.RealtimeStopId == "905").Confidence).IsEqualTo(MatchConfidence.High);
            Check.That(v_Matches.Single(p_M => p_M.RealtimeStopId == "906").Confidence).IsEqualTo(MatchConfidence.Low);
            Check.That((await v_References.GetLineMatchesAsync(CancellationToken.None)).Single().Line.Id).IsEqualTo("L1");
        }

        [Fact]
        public async Task Vehicle_History_Ignores_Already_Stored_Positions_And_Purges_Old_Ones()
        {
            await ResetAsync();
            DateTime v_Now = new(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);
            VehiclePosition[] v_Positions =
            [
                new("718", "718", "T1", "L1", 49.8, 2.3, 90, 10, null, v_Now),
                new("718", "718", "T1", "L1", 49.8, 2.3, 90, 10, null, v_Now.AddDays(-31))
            ];

            await using AppDbContext v_Context = NewContext();
            VehicleHistoryRepository v_History = new(v_Context);
            Check.That(await v_History.AppendAsync(v_Positions, CancellationToken.None)).IsEqualTo(2);
            Check.That(await v_History.AppendAsync(v_Positions, CancellationToken.None)).IsEqualTo(0);
            Check.That(await v_History.PurgeOlderThanAsync(v_Now.AddDays(-30), CancellationToken.None)).IsEqualTo(1);
            Check.That((await v_Context.VehiclePositions.SingleAsync()).PositionTimestampUtc).IsEqualTo(v_Now);
        }

        [Fact]
        public async Task Favorites_Are_Unique_And_Isolated_Per_User_And_Deleted_With_Account()
        {
            await ResetAsync();
            await using AppDbContext v_Context = NewContext();
            v_Context.Users.AddRange(new AmetisAPI.Infrastructure.Data.EntityFramework.Entities.AppUser { Id = "u1", UserName = "u1" }, new AmetisAPI.Infrastructure.Data.EntityFramework.Entities.AppUser { Id = "u2", UserName = "u2" });
            await v_Context.SaveChangesAsync();
            FavoriteRepository v_Favorites = new(v_Context);
            DateTime v_Now = DateTime.UtcNow;

            Check.That(await v_Favorites.AddAsync("u1", FavoriteKind.Stop, "AREA", v_Now, CancellationToken.None)).IsTrue();
            Check.That(await v_Favorites.AddAsync("u1", FavoriteKind.Stop, "AREA", v_Now, CancellationToken.None)).IsFalse();
            Check.That(await v_Favorites.AddAsync("u2", FavoriteKind.Line, "L1", v_Now, CancellationToken.None)).IsTrue();

            Check.That((await v_Favorites.ListAsync("u1", CancellationToken.None)).Select(p_F => p_F.TargetId)).ContainsExactly("AREA");
            Check.That(await v_Favorites.RemoveAsync("u1", FavoriteKind.Line, "L1", CancellationToken.None)).IsFalse();

            await v_Context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUsers WHERE Id = 'u2'");
            Check.That(await v_Favorites.ListAsync("u2", CancellationToken.None)).IsEmpty();
        }
    }
}
