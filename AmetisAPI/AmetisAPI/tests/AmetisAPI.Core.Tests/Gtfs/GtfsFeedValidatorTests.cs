using AmetisAPI.Core.Gtfs;
using AmetisAPI.Core.Tests.Utilities;
using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Exceptions;

namespace AmetisAPI.Core.Tests.Gtfs
{
    public class GtfsFeedValidatorTests
    {
        [Fact]
        public void Valid_Feed_Is_Kept_Whole_Without_Rejection()
        {
            GtfsFeed v_Result = GtfsFeedValidator.Validate(GtfsFeedBuilder.Valid());

            Check.That(v_Result.Rejections).IsEmpty();
            Check.That(v_Result.StopTimes).HasSize(4);
            Check.That(v_Result.Trips).HasSize(2);
        }

        [Fact]
        public void Duplicate_Route_Keeps_First_And_Logs_Second()
        {
            GtfsFeed v_Feed = GtfsFeedBuilder.Valid();
            v_Feed.Routes.Add(v_Feed.Routes[0] with { ShortName = "doublon" });

            GtfsFeed v_Result = GtfsFeedValidator.Validate(v_Feed);

            Check.That(v_Result.Routes).HasSize(1);
            Check.That(v_Result.Routes[0].ShortName).IsEqualTo("1");
            Check.That(v_Result.Rejections.Single().Reason).Contains("route_id L1 en double");
        }

        [Fact]
        public void Trip_With_Unknown_Route_Or_Service_Is_Rejected_With_Its_Stop_Times()
        {
            GtfsFeed v_Feed = GtfsFeedBuilder.Valid();
            v_Feed.Trips.Add(new GtfsTrip("T3", "INCONNUE", "SEM", null, 0, null));
            v_Feed.Trips.Add(new GtfsTrip("T4", "L1", "SANS_CALENDRIER", null, 0, null));
            for (int v_I = 0; v_I < 200; v_I++)
                v_Feed.StopTimes.Add(new GtfsStopTime("T1", 100 + v_I, "Q1", 0, 0, 0, 0, null));
            v_Feed.StopTimes.Add(new GtfsStopTime("T3", 1, "Q1", 0, 0, 0, 0, null));

            GtfsFeed v_Result = GtfsFeedValidator.Validate(v_Feed);

            Check.That(v_Result.Trips.Select(p_T => p_T.TripId)).ContainsExactly("T1", "T2");
            Check.That(v_Result.Rejections.Select(p_R => p_R.Reason)).Contains(
                "trip_id=T3 : route_id INCONNUE inconnu",
                "trip_id=T4 : service_id SANS_CALENDRIER absent du calendrier",
                "trip_id T3 inconnu ou rejeté");
        }

        [Fact]
        public void Unknown_Parent_Station_Is_Detached_And_Logged_Not_Silently()
        {
            GtfsFeed v_Feed = GtfsFeedBuilder.Valid();
            v_Feed.Stops[2] = v_Feed.Stops[2] with { ParentStationId = "FANTOME" };

            GtfsFeed v_Result = GtfsFeedValidator.Validate(v_Feed);

            Check.That(v_Result.Stops.Single(p_S => p_S.StopId == "Q2").ParentStationId).IsNull();
            Check.That(v_Result.Rejections.Single().Reason).Contains("parent_station FANTOME inconnu");
        }

        [Fact]
        public void Duplicate_Stop_Sequence_Is_Rejected()
        {
            GtfsFeed v_Feed = GtfsFeedBuilder.Valid();
            for (int v_I = 0; v_I < 100; v_I++)
                v_Feed.StopTimes.Add(new GtfsStopTime("T2", 10 + v_I, "Q1", 0, 0, 0, 0, null));
            v_Feed.StopTimes.Add(new GtfsStopTime("T1", 1, "Q2", 1, 1, 0, 0, null));

            GtfsFeed v_Result = GtfsFeedValidator.Validate(v_Feed);

            Check.That(v_Result.Rejections.Single().Reason).IsEqualTo("trip_id=T1 : stop_sequence 1 en double");
        }

        [Fact]
        public void Too_Many_Rejected_Stop_Times_Fails_The_Whole_Feed()
        {
            GtfsFeed v_Feed = GtfsFeedBuilder.Valid();
            v_Feed.StopTimes.Add(new GtfsStopTime("T1", 3, "ARRET_INCONNU", 0, 0, 0, 0, null));

            Check.ThatCode(() => GtfsFeedValidator.Validate(v_Feed)).Throws<GtfsFormatException>();
        }

        [Fact]
        public void Feed_Without_Trips_After_Validation_Fails()
        {
            GtfsFeed v_Feed = GtfsFeedBuilder.Valid();
            v_Feed.Calendars.Clear();
            v_Feed.CalendarDates.Clear();

            Check.ThatCode(() => GtfsFeedValidator.Validate(v_Feed)).Throws<GtfsFormatException>();
        }
    }
}
