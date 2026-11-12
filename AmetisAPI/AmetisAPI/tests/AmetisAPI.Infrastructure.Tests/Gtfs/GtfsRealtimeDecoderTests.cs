using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Infrastructure.Gtfs;
using Google.Protobuf;

namespace AmetisAPI.Infrastructure.Tests.Gtfs
{
    public class GtfsRealtimeDecoderTests
    {
        private const long Timestamp = 1794480756; // 2026-11-12T10:52:36Z

        private static byte[] Message(Action<CodedOutputStream> p_Write)
        {
            using MemoryStream v_Stream = new();
            CodedOutputStream v_Out = new(v_Stream);
            p_Write(v_Out);
            v_Out.Flush();
            return v_Stream.ToArray();
        }

        private static void Bytes(CodedOutputStream p_Out, int p_Field, byte[] p_Value)
        {
            p_Out.WriteTag(p_Field, WireFormat.WireType.LengthDelimited);
            p_Out.WriteBytes(ByteString.CopyFrom(p_Value));
        }

        private static void Text(CodedOutputStream p_Out, int p_Field, string p_Value)
        {
            p_Out.WriteTag(p_Field, WireFormat.WireType.LengthDelimited);
            p_Out.WriteString(p_Value);
        }

        private static void Varint(CodedOutputStream p_Out, int p_Field, ulong p_Value)
        {
            p_Out.WriteTag(p_Field, WireFormat.WireType.Varint);
            p_Out.WriteUInt64(p_Value);
        }

        private static byte[] Feed(params byte[][] p_Entities) => Message(p_O =>
        {
            Bytes(p_O, 1, Message(p_H => { Text(p_H, 1, "2.0"); Varint(p_H, 3, Timestamp); }));
            foreach (byte[] v_Entity in p_Entities)
                Bytes(p_O, 2, v_Entity);
        });

        private static byte[] Trip(string p_TripId, string p_RouteId) => Message(p_T => { Text(p_T, 1, p_TripId); Text(p_T, 5, p_RouteId); });

        private static byte[] StopTimeUpdate(string p_StopId, long p_Time, int p_Delay) => Message(p_U =>
        {
            Bytes(p_U, 3, Message(p_E => { p_E.WriteTag(1, WireFormat.WireType.Varint); p_E.WriteInt32(p_Delay); p_E.WriteTag(2, WireFormat.WireType.Varint); p_E.WriteInt64(p_Time); }));
            Text(p_U, 4, p_StopId);
        });

        [Fact]
        public void Vehicle_Position_Is_Decoded_With_Trip_Speed_And_Timestamp()
        {
            byte[] v_Vehicle = Message(p_V =>
            {
                Bytes(p_V, 1, Trip("2-5A-1", "5A"));
                Bytes(p_V, 2, Message(p_P =>
                {
                    p_P.WriteTag(1, WireFormat.WireType.Fixed32); p_P.WriteFloat(49.85f);
                    p_P.WriteTag(2, WireFormat.WireType.Fixed32); p_P.WriteFloat(2.38f);
                    p_P.WriteTag(3, WireFormat.WireType.Fixed32); p_P.WriteFloat(90f);
                    p_P.WriteTag(5, WireFormat.WireType.Fixed32); p_P.WriteFloat(10f);
                }));
                Varint(p_V, 5, Timestamp - 10);
                Text(p_V, 7, "905");
                Bytes(p_V, 8, Message(p_D => { Text(p_D, 1, "718"); Text(p_D, 2, "Bus 718"); }));
                Varint(p_V, 9, 1); // occupancy_status : champ ignoré
            });

            RealtimeFeed v_Feed = new GtfsRealtimeDecoder().Decode(Feed(Message(p_E => { Text(p_E, 1, "e1"); Bytes(p_E, 4, v_Vehicle); })));

            Check.That(v_Feed.TimestampUtc).IsEqualTo(new DateTime(2026, 11, 12, 10, 52, 36, DateTimeKind.Utc));
            VehiclePosition v_Position = v_Feed.Vehicles.Single();
            Check.That(v_Position.VehicleId).IsEqualTo("718");
            Check.That(v_Position.TripId).IsEqualTo("2-5A-1");
            Check.That(v_Position.RouteId).IsEqualTo("5A");
            Check.That(v_Position.Latitude).IsCloseTo(49.85, 0.0001);
            Check.That(v_Position.SpeedMetersPerSecond).IsEqualTo(10);
            Check.That(v_Position.StopId).IsEqualTo("905");
            Check.That(v_Position.TimestampUtc).IsEqualTo(new DateTime(2026, 11, 12, 10, 52, 26, DateTimeKind.Utc));
            Check.That(v_Feed.Anomalies).IsEmpty();
        }

        [Fact]
        public void Vehicle_Without_Position_Is_Ignored_And_Reported()
        {
            byte[] v_Vehicle = Message(p_V => Bytes(p_V, 8, Message(p_D => Text(p_D, 1, "42"))));

            RealtimeFeed v_Feed = new GtfsRealtimeDecoder().Decode(Feed(Message(p_E => Bytes(p_E, 4, v_Vehicle))));

            Check.That(v_Feed.Vehicles).IsEmpty();
            Check.That(v_Feed.Anomalies.Single()).IsEqualTo("Véhicule 42 : position absente, ignoré");
        }

        [Fact]
        public void Trip_Update_Keeps_Published_Times_And_Reports_Non_Increasing_Sequence()
        {
            byte[] v_Update = Message(p_U =>
            {
                Bytes(p_U, 1, Trip("2-11-1", "11"));
                Bytes(p_U, 2, StopTimeUpdate("A", Timestamp + 120, 30));
                Bytes(p_U, 2, StopTimeUpdate("B", Timestamp + 60, 0));
                Bytes(p_U, 3, Message(p_D => Text(p_D, 1, "149")));
            });

            RealtimeFeed v_Feed = new GtfsRealtimeDecoder().Decode(Feed(Message(p_E => Bytes(p_E, 3, v_Update))));

            Check.That(v_Feed.Predictions).HasSize(2);
            Check.That(v_Feed.Predictions[0].DelaySeconds).IsEqualTo(30);
            Check.That(v_Feed.Predictions[1].DepartureUtc).IsEqualTo(new DateTime(2026, 11, 12, 10, 53, 36, DateTimeKind.Utc));
            Check.That(v_Feed.Predictions[1].VehicleId).IsEqualTo("149");
            Check.That(v_Feed.Anomalies.Single()).IsEqualTo("Course 2-11-1 : heures non croissantes à l'arrêt B");
        }

        [Fact]
        public void Alert_Prefers_French_Translation()
        {
            static byte[] Translated(params (string Text, string Lang)[] p_Items) => Message(p_T =>
            {
                foreach ((string v_Text, string v_Lang) in p_Items)
                    Bytes(p_T, 1, Message(p_X => { Text(p_X, 1, v_Text); Text(p_X, 2, v_Lang); }));
            });
            byte[] v_Alert = Message(p_A =>
            {
                Bytes(p_A, 1, Message(p_R => { Varint(p_R, 1, Timestamp); Varint(p_R, 2, Timestamp + 3600); }));
                Bytes(p_A, 5, Message(p_S => Text(p_S, 2, "N1")));
                Bytes(p_A, 10, Translated(("Works", "en"), ("Travaux", "fr")));
            });

            ServiceAlert v_Result = new GtfsRealtimeDecoder().Decode(Feed(Message(p_E => { Text(p_E, 1, "a1"); Bytes(p_E, 5, v_Alert); }))).Alerts.Single();

            Check.That(v_Result.Id).IsEqualTo("a1");
            Check.That(v_Result.Header).IsEqualTo("Travaux");
            Check.That(v_Result.RouteIds).ContainsExactly("N1");
            Check.That(v_Result.EndUtc).IsEqualTo(new DateTime(2026, 11, 12, 11, 52, 36, DateTimeKind.Utc));
        }

        [Fact]
        public void Json_Content_Is_Not_Decodable()
        {
            byte[] v_Json = System.Text.Encoding.UTF8.GetBytes("{\n  \"header\": {\"gtfsRealtimeVersion\": \"2.0\"}}");

            Check.ThatCode(() => new GtfsRealtimeDecoder().Decode(v_Json)).ThrowsAny();
        }
    }
}
