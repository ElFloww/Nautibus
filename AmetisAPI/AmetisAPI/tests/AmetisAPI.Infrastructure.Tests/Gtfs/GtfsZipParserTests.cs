using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Exceptions;
using AmetisAPI.Infrastructure.Gtfs;

namespace AmetisAPI.Infrastructure.Tests.Gtfs
{
    public class GtfsZipParserTests
    {
        private static GtfsFeed Parse(Dictionary<string, string> p_Files) => new GtfsZipParser().Parse(GtfsZipFixture.Zip(p_Files));

        [Fact]
        public void Valid_Archive_Is_Read_Including_Times_Beyond_24h_And_Quoted_Names()
        {
            GtfsFeed v_Feed = Parse(GtfsZipFixture.Files());

            Check.That(v_Feed.Routes.Single().LongName).IsEqualTo("Gare <> CHU, Sud");
            Check.That(v_Feed.Stops.Select(p_S => p_S.StopId)).ContainsExactly("AREA", "Q1", "Q2");
            Check.That(v_Feed.Stops[2].LocationType).IsEqualTo(0);
            Check.That(v_Feed.StopTimes[1].DepartureSeconds).IsEqualTo(24 * 3600 + 37 * 60);
            Check.That(v_Feed.Calendars.Single().DaysMask).IsEqualTo(0b0011111);
            Check.That(v_Feed.CalendarDates.Single().Date).IsEqualTo(new DateOnly(2026, 11, 11));
            Check.That(v_Feed.ShapePoints).HasSize(2);
        }

        [Fact]
        public void Unreadable_Rows_Are_Rejected_With_File_Line_And_Reason()
        {
            GtfsFeed v_Feed = Parse(GtfsZipFixture.Files());

            Check.That(v_Feed.StopTimes).HasSize(2);
            Check.That(v_Feed.Rejections).ContainsExactly(
                new GtfsRejection("stop_times.txt", 4, "desserte de zone flexible (location_id) non prise en charge"),
                new GtfsRejection("stop_times.txt", 5, "arrival_time « 9h00 » n'est pas une heure H:MM:SS"));
        }

        [Fact]
        public void Missing_Required_File_Fails_The_Import()
        {
            Dictionary<string, string> v_Files = GtfsZipFixture.Files();
            v_Files.Remove("stops.txt");

            Check.ThatCode(() => Parse(v_Files)).Throws<GtfsFormatException>().WithMessage("Fichier obligatoire stops.txt absent");
        }

        [Fact]
        public void Missing_Required_Column_Fails_The_Import()
        {
            Dictionary<string, string> v_Files = GtfsZipFixture.Files();
            v_Files["trips.txt"] = "route_id,trip_id\nL1,T1\n";

            Check.ThatCode(() => Parse(v_Files)).Throws<GtfsFormatException>().WithMessage("trips.txt : colonne obligatoire « service_id » absente");
        }

        [Fact]
        public void No_Calendar_At_All_Fails_The_Import()
        {
            Dictionary<string, string> v_Files = GtfsZipFixture.Files();
            v_Files.Remove("calendar.txt");
            v_Files.Remove("calendar_dates.txt");

            Check.ThatCode(() => Parse(v_Files)).Throws<GtfsFormatException>();
        }

        [Fact]
        public void Absent_Shapes_Is_Allowed()
        {
            Dictionary<string, string> v_Files = GtfsZipFixture.Files();
            v_Files.Remove("shapes.txt");

            Check.That(Parse(v_Files).ShapePoints).IsEmpty();
        }

        [Fact]
        public void Out_Of_Range_Coordinates_Reject_The_Stop()
        {
            Dictionary<string, string> v_Files = GtfsZipFixture.Files();
            v_Files["stops.txt"] += "BAD,Nulle part,95,2.3,0,\n";

            GtfsFeed v_Feed = Parse(v_Files);

            Check.That(v_Feed.Stops.Select(p_S => p_S.StopId)).Not.Contains("BAD");
            Check.That(v_Feed.Rejections.Select(p_R => p_R.Reason)).Contains("coordonnées hors limites (95, 2.3)");
        }

        [Fact]
        public void Not_A_Zip_Fails_The_Import()
        {
            Check.ThatCode(() => new GtfsZipParser().Parse(new MemoryStream([1, 2, 3]))).Throws<GtfsFormatException>();
        }
    }
}
