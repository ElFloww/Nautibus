using AmetisAPI.Infrastructure.Data.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AmetisAPI.Infrastructure.Data.EntityFramework
{
    /// <summary>
    /// Contexte unique : Identity (schéma dbo), réseau GTFS (gtfs), temps réel (rt), données utilisateur (app).
    /// Le schéma de référence est le projet AmetisAPI.Database (DACPAC) ; ce mapping doit lui correspondre.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> p_Options) : IdentityDbContext<AppUser>(p_Options)
    {
        public const string ConnectionStringName = "Database";

        private const int IdLength = 255;
        private const int NameLength = 255;

        public DbSet<FeedVersionEntity> FeedVersions => Set<FeedVersionEntity>();
        public DbSet<ImportRejectionEntity> ImportRejections => Set<ImportRejectionEntity>();
        public DbSet<RouteEntity> Routes => Set<RouteEntity>();
        public DbSet<StopEntity> Stops => Set<StopEntity>();
        public DbSet<TripEntity> Trips => Set<TripEntity>();
        public DbSet<StopTimeEntity> StopTimes => Set<StopTimeEntity>();
        public DbSet<CalendarEntity> Calendars => Set<CalendarEntity>();
        public DbSet<CalendarDateEntity> CalendarDates => Set<CalendarDateEntity>();
        public DbSet<ShapePointEntity> ShapePoints => Set<ShapePointEntity>();
        public DbSet<StopRouteEntity> StopRoutes => Set<StopRouteEntity>();
        public DbSet<StopMatchEntity> StopMatches => Set<StopMatchEntity>();
        public DbSet<RouteMatchEntity> RouteMatches => Set<RouteMatchEntity>();
        public DbSet<VehiclePositionEntity> VehiclePositions => Set<VehiclePositionEntity>();
        public DbSet<FavoriteEntity> Favorites => Set<FavoriteEntity>();
        public DbSet<StopPassageEntity> StopPassages => Set<StopPassageEntity>();
        public DbSet<NetworkEventEntity> NetworkEvents => Set<NetworkEventEntity>();
        public DbSet<PushSubscriptionEntity> PushSubscriptions => Set<PushSubscriptionEntity>();

        /// <summary>Toutes les dates sont stockées en UTC : elles sont relues comme telles (SQL Server ne garde pas le genre).</summary>
        protected override void ConfigureConventions(ModelConfigurationBuilder p_Builder)
        {
            p_Builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
            p_Builder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
        }

        [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FeedVersionEntity>(p_E =>
            {
                p_E.ToTable("FeedVersion", "gtfs");
                p_E.Property(p_X => p_X.SourceUrl).HasMaxLength(1000);
                p_E.Property(p_X => p_X.ContentHash).HasMaxLength(64);
                p_E.HasIndex(p_X => new { p_X.Source, p_X.IsActive });
            });
            modelBuilder.Entity<ImportRejectionEntity>(p_E =>
            {
                p_E.ToTable("ImportRejection", "gtfs");
                p_E.Property(p_X => p_X.FileName).HasMaxLength(100);
                p_E.Property(p_X => p_X.Reason).HasMaxLength(1000);
                p_E.HasIndex(p_X => p_X.FeedVersionId);
            });
            modelBuilder.Entity<RouteEntity>(p_E =>
            {
                p_E.ToTable("Route", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.RouteId });
                p_E.Property(p_X => p_X.RouteId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.ShortName).HasMaxLength(NameLength);
                p_E.Property(p_X => p_X.LongName).HasMaxLength(NameLength);
                p_E.Property(p_X => p_X.Color).HasMaxLength(20);
                p_E.Property(p_X => p_X.TextColor).HasMaxLength(20);
            });
            modelBuilder.Entity<StopEntity>(p_E =>
            {
                p_E.ToTable("Stop", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.StopId });
                p_E.Property(p_X => p_X.StopId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.Code).HasMaxLength(NameLength);
                p_E.Property(p_X => p_X.Name).HasMaxLength(NameLength);
                p_E.Property(p_X => p_X.ParentStationId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.Location).HasColumnType("geography");
                p_E.HasIndex(p_X => new { p_X.FeedVersionId, p_X.ParentStationId });
            });
            modelBuilder.Entity<TripEntity>(p_E =>
            {
                p_E.ToTable("Trip", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.TripId });
                p_E.Property(p_X => p_X.TripId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.RouteId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.ServiceId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.ShapeId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.Headsign).HasMaxLength(NameLength);
                p_E.Property(p_X => p_X.Destination).HasMaxLength(NameLength);
                p_E.HasIndex(p_X => new { p_X.FeedVersionId, p_X.RouteId });
            });
            modelBuilder.Entity<StopTimeEntity>(p_E =>
            {
                p_E.ToTable("StopTime", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.TripId, p_X.StopSequence });
                p_E.Property(p_X => p_X.TripId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.StopId).HasMaxLength(IdLength);
                p_E.HasIndex(p_X => new { p_X.FeedVersionId, p_X.StopId, p_X.DepartureSeconds });
            });
            modelBuilder.Entity<CalendarEntity>(p_E =>
            {
                p_E.ToTable("Calendar", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.ServiceId });
                p_E.Property(p_X => p_X.ServiceId).HasMaxLength(IdLength);
            });
            modelBuilder.Entity<CalendarDateEntity>(p_E =>
            {
                p_E.ToTable("CalendarDate", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.ServiceId, p_X.Date });
                p_E.Property(p_X => p_X.ServiceId).HasMaxLength(IdLength);
            });
            modelBuilder.Entity<ShapePointEntity>(p_E =>
            {
                p_E.ToTable("ShapePoint", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.ShapeId, p_X.Sequence });
                p_E.Property(p_X => p_X.ShapeId).HasMaxLength(IdLength);
            });
            modelBuilder.Entity<StopRouteEntity>(p_E =>
            {
                p_E.ToTable("StopRoute", "gtfs");
                p_E.HasKey(p_X => new { p_X.FeedVersionId, p_X.StopId, p_X.RouteId });
                p_E.Property(p_X => p_X.StopId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.RouteId).HasMaxLength(IdLength);
            });
            modelBuilder.Entity<StopMatchEntity>(p_E =>
            {
                p_E.ToTable("StopMatch", "gtfs");
                p_E.HasKey(p_X => p_X.RealtimeStopId);
                p_E.Property(p_X => p_X.RealtimeStopId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.NetworkStopId).HasMaxLength(IdLength);
                p_E.HasIndex(p_X => p_X.NetworkStopId);
            });
            modelBuilder.Entity<RouteMatchEntity>(p_E =>
            {
                p_E.ToTable("RouteMatch", "gtfs");
                p_E.HasKey(p_X => p_X.RealtimeRouteId);
                p_E.Property(p_X => p_X.RealtimeRouteId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.NetworkRouteId).HasMaxLength(IdLength);
            });
            modelBuilder.Entity<VehiclePositionEntity>(p_E =>
            {
                p_E.ToTable("VehiclePosition", "rt");
                p_E.Property(p_X => p_X.VehicleId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.TripId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.RouteId).HasMaxLength(IdLength);
                p_E.HasIndex(p_X => new { p_X.VehicleId, p_X.PositionTimestampUtc }).IsUnique();
                p_E.HasIndex(p_X => p_X.PositionTimestampUtc);
            });
            modelBuilder.Entity<FavoriteEntity>(p_E =>
            {
                p_E.ToTable("Favorite", "app");
                p_E.Property(p_X => p_X.TargetId).HasMaxLength(IdLength);
                p_E.HasIndex(p_X => new { p_X.UserId, p_X.Kind, p_X.TargetId }).IsUnique();
                p_E.HasOne<AppUser>().WithMany().HasForeignKey(p_X => p_X.UserId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<StopPassageEntity>(p_E =>
            {
                p_E.ToTable("StopPassage", "rt");
                p_E.Property(p_X => p_X.TripId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.RouteId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.StopId).HasMaxLength(IdLength);
                p_E.Property(p_X => p_X.VehicleId).HasMaxLength(IdLength);
                p_E.HasIndex(p_X => new { p_X.TripId, p_X.StopId, p_X.ScheduledUtc }).IsUnique();
                p_E.HasIndex(p_X => new { p_X.StopId, p_X.ExpectedUtc });
                p_E.HasIndex(p_X => p_X.ScheduledUtc);
            });
            modelBuilder.Entity<NetworkEventEntity>(p_E =>
            {
                p_E.ToTable("NetworkEvent", "rt");
                p_E.Property(p_X => p_X.ExternalId).HasMaxLength(400);
                p_E.Property(p_X => p_X.Url).HasMaxLength(1000);
                p_E.HasIndex(p_X => new { p_X.Kind, p_X.ClosedUtc });
            });
            modelBuilder.Entity<PushSubscriptionEntity>(p_E =>
            {
                p_E.ToTable("PushSubscription", "app");
                p_E.Property(p_X => p_X.Endpoint).HasMaxLength(2000);
                p_E.Property(p_X => p_X.EndpointHash).HasMaxLength(64);
                p_E.Property(p_X => p_X.P256dh).HasMaxLength(200);
                p_E.Property(p_X => p_X.Auth).HasMaxLength(100);
                p_E.HasIndex(p_X => p_X.EndpointHash).IsUnique();
                p_E.HasOne<AppUser>().WithMany().HasForeignKey(p_X => p_X.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
