using AmetisAPI.Domain.Bo.Realtime;
using AmetisAPI.Domain.Interfaces.Realtime;
using Google.Protobuf;

namespace AmetisAPI.Infrastructure.Gtfs
{
    /// <summary>
    /// Décodage du FeedMessage GTFS-RT (gtfs-realtime.proto) champ par champ, sans code généré.
    /// Seuls les champs utilisés sont lus ; les autres sont ignorés. Les incohérences sont consignées dans Anomalies.
    /// </summary>
    internal class GtfsRealtimeDecoder : IGtfsRealtimeDecoder
    {
        public RealtimeFeed Decode(byte[] p_Content)
        {
            ArgumentNullException.ThrowIfNull(p_Content);
            DateTime? v_HeaderTime = null;
            List<ByteString> v_Entities = [];
            Each(new CodedInputStream(p_Content), (p_Field, p_In) =>
            {
                switch (p_Field)
                {
                    case 1:
                        Each(p_In.ReadBytes(), (p_F, p_H) => p_F == 3 && Assign(ref v_HeaderTime, ToUtc(p_H.ReadUInt64())));
                        return true;
                    case 2:
                        v_Entities.Add(p_In.ReadBytes());
                        return true;
                    default:
                        return false;
                }
            });

            List<VehiclePosition> v_Vehicles = [];
            List<StopTimePrediction> v_Predictions = [];
            List<ServiceAlert> v_Alerts = [];
            List<string> v_Anomalies = [];
            foreach (ByteString v_Entity in v_Entities)
            {
                string v_Id = null;
                Each(v_Entity, (p_Field, p_In) =>
                {
                    switch (p_Field)
                    {
                        case 1:
                            v_Id = p_In.ReadString();
                            return true;
                        case 3:
                            ReadTripUpdate(p_In.ReadBytes(), v_Predictions, v_Anomalies);
                            return true;
                        case 4:
                            ReadVehicle(p_In.ReadBytes(), v_HeaderTime, v_Vehicles, v_Anomalies);
                            return true;
                        case 5:
                            v_Alerts.Add(ReadAlert(v_Id, p_In.ReadBytes()));
                            return true;
                        default:
                            return false;
                    }
                });
            }
            return new RealtimeFeed(v_HeaderTime, v_Vehicles, v_Predictions, v_Alerts, v_Anomalies);
        }

        private static void ReadVehicle(ByteString p_Bytes, DateTime? p_HeaderTime, List<VehiclePosition> p_Vehicles, List<string> p_Anomalies)
        {
            (string TripId, string RouteId) v_Trip = default;
            (string Id, string Label) v_Vehicle = default;
            float? v_Lat = null, v_Lon = null, v_Bearing = null, v_Speed = null;
            DateTime? v_Time = null;
            string v_StopId = null;
            Each(p_Bytes, (p_Field, p_In) =>
            {
                switch (p_Field)
                {
                    case 1: v_Trip = ReadTrip(p_In.ReadBytes()); return true;
                    case 2:
                        Each(p_In.ReadBytes(), (p_F, p_P) => p_F switch
                        {
                            1 => Assign(ref v_Lat, p_P.ReadFloat()),
                            2 => Assign(ref v_Lon, p_P.ReadFloat()),
                            3 => Assign(ref v_Bearing, p_P.ReadFloat()),
                            5 => Assign(ref v_Speed, p_P.ReadFloat()),
                            _ => false
                        });
                        return true;
                    case 5: v_Time = ToUtc(p_In.ReadUInt64()); return true;
                    case 7: v_StopId = p_In.ReadString(); return true;
                    case 8:
                        Each(p_In.ReadBytes(), (p_F, p_V) => p_F switch
                        {
                            1 => Assign(ref v_Vehicle.Id, p_V.ReadString()),
                            2 => Assign(ref v_Vehicle.Label, p_V.ReadString()),
                            _ => false
                        });
                        return true;
                    default: return false;
                }
            });

            if (v_Lat is null || v_Lon is null)
            {
                p_Anomalies.Add($"Véhicule {v_Vehicle.Id} : position absente, ignoré");
                return;
            }
            if (v_Vehicle.Id is null)
            {
                p_Anomalies.Add("Véhicule sans identifiant, ignoré");
                return;
            }
            if (v_Time is null)
                p_Anomalies.Add($"Véhicule {v_Vehicle.Id} : horodatage absent, horodatage du flux utilisé");
            DateTime? v_Timestamp = v_Time ?? p_HeaderTime;
            if (v_Timestamp is null)
                return;
            p_Vehicles.Add(new VehiclePosition(v_Vehicle.Id, v_Vehicle.Label, v_Trip.TripId, v_Trip.RouteId, v_Lat.Value, v_Lon.Value,
                v_Bearing, v_Speed, v_StopId, v_Timestamp.Value));
        }

        private static void ReadTripUpdate(ByteString p_Bytes, List<StopTimePrediction> p_Predictions, List<string> p_Anomalies)
        {
            (string TripId, string RouteId) v_Trip = default;
            string v_VehicleId = null;
            List<ByteString> v_Updates = [];
            Each(p_Bytes, (p_Field, p_In) =>
            {
                switch (p_Field)
                {
                    case 1: v_Trip = ReadTrip(p_In.ReadBytes()); return true;
                    case 2: v_Updates.Add(p_In.ReadBytes()); return true;
                    case 3: Each(p_In.ReadBytes(), (p_F, p_V) => p_F == 1 && Assign(ref v_VehicleId, p_V.ReadString())); return true;
                    default: return false;
                }
            });
            if (v_Trip.TripId is null)
            {
                p_Anomalies.Add("Trip update sans trip_id, ignoré");
                return;
            }

            DateTime? v_Previous = null;
            foreach (ByteString v_Update in v_Updates)
            {
                int? v_Sequence = null;
                string v_StopId = null;
                int v_Relationship = 0;
                (DateTime? Time, int? Delay) v_Arrival = default, v_Departure = default;
                Each(v_Update, (p_Field, p_In) =>
                {
                    switch (p_Field)
                    {
                        case 1: v_Sequence = (int)p_In.ReadUInt32(); return true;
                        case 2: v_Arrival = ReadEvent(p_In.ReadBytes()); return true;
                        case 3: v_Departure = ReadEvent(p_In.ReadBytes()); return true;
                        case 4: v_StopId = p_In.ReadString(); return true;
                        case 5: v_Relationship = p_In.ReadEnum(); return true;
                        default: return false;
                    }
                });
                // 1 = SKIPPED, 2 = NO_DATA : pas une prévision de passage.
                if (v_Relationship is 1 or 2)
                    continue;

                DateTime? v_Time = v_Departure.Time ?? v_Arrival.Time;
                if (v_Time < v_Previous)
                    p_Anomalies.Add($"Course {v_Trip.TripId} : heures non croissantes à l'arrêt {v_StopId}");
                v_Previous = v_Time ?? v_Previous;

                p_Predictions.Add(new StopTimePrediction(v_Trip.TripId, v_Trip.RouteId, v_StopId, v_Sequence,
                    v_Arrival.Time, v_Departure.Time, v_Departure.Delay ?? v_Arrival.Delay, v_VehicleId));
            }
        }

        private static ServiceAlert ReadAlert(string p_Id, ByteString p_Bytes)
        {
            DateTime? v_Start = null, v_End = null;
            string v_Header = null, v_Description = null, v_Url = null;
            List<string> v_Routes = [], v_Stops = [];
            Each(p_Bytes, (p_Field, p_In) =>
            {
                switch (p_Field)
                {
                    case 1:
                        // Seule la première période est retenue.
                        Each(p_In.ReadBytes(), (p_F, p_T) => p_F switch
                        {
                            1 => Assign(ref v_Start, v_Start ?? ToUtc(p_T.ReadUInt64())),
                            2 => Assign(ref v_End, v_End ?? ToUtc(p_T.ReadUInt64())),
                            _ => false
                        });
                        return true;
                    case 5:
                        Each(p_In.ReadBytes(), (p_F, p_E) =>
                        {
                            if (p_F == 2) { v_Routes.Add(p_E.ReadString()); return true; }
                            if (p_F == 5) { v_Stops.Add(p_E.ReadString()); return true; }
                            return false;
                        });
                        return true;
                    case 8: v_Url = ReadTranslated(p_In.ReadBytes()); return true;
                    case 10: v_Header = ReadTranslated(p_In.ReadBytes()); return true;
                    case 11: v_Description = ReadTranslated(p_In.ReadBytes()); return true;
                    default: return false;
                }
            });
            return new ServiceAlert(p_Id, v_Header, v_Description, v_Url, v_Start, v_End, v_Routes, v_Stops);
        }

        /// <summary>Texte français si présent, sinon la première traduction.</summary>
        private static string ReadTranslated(ByteString p_Bytes)
        {
            string v_First = null, v_French = null;
            Each(p_Bytes, (p_Field, p_In) =>
            {
                if (p_Field != 1)
                    return false;
                string v_Text = null, v_Lang = null;
                Each(p_In.ReadBytes(), (p_F, p_T) => p_F switch
                {
                    1 => Assign(ref v_Text, p_T.ReadString()),
                    2 => Assign(ref v_Lang, p_T.ReadString()),
                    _ => false
                });
                v_First ??= v_Text;
                if (v_Lang?.StartsWith("fr", StringComparison.OrdinalIgnoreCase) == true)
                    v_French ??= v_Text;
                return true;
            });
            return v_French ?? v_First;
        }

        private static (string TripId, string RouteId) ReadTrip(ByteString p_Bytes)
        {
            (string TripId, string RouteId) v_Trip = default;
            Each(p_Bytes, (p_Field, p_In) => p_Field switch
            {
                1 => Assign(ref v_Trip.TripId, p_In.ReadString()),
                5 => Assign(ref v_Trip.RouteId, p_In.ReadString()),
                _ => false
            });
            return v_Trip;
        }

        private static (DateTime? Time, int? Delay) ReadEvent(ByteString p_Bytes)
        {
            (DateTime? Time, int? Delay) v_Event = default;
            Each(p_Bytes, (p_Field, p_In) => p_Field switch
            {
                1 => Assign(ref v_Event.Delay, p_In.ReadInt32()),
                2 => Assign(ref v_Event.Time, ToUtc((ulong)p_In.ReadInt64())),
                _ => false
            });
            return v_Event;
        }

        private static DateTime ToUtc(ulong p_Seconds) => DateTimeOffset.FromUnixTimeSeconds((long)p_Seconds).UtcDateTime;

        private static bool Assign<T>(ref T p_Target, T p_Value)
        {
            p_Target = p_Value;
            return true;
        }

        private static void Each(ByteString p_Bytes, Func<int, CodedInputStream, bool> p_OnField) => Each(p_Bytes.CreateCodedInput(), p_OnField);

        /// <summary>Parcourt les champs d'un message ; un champ non lu par le rappel est ignoré.</summary>
        private static void Each(CodedInputStream p_In, Func<int, CodedInputStream, bool> p_OnField)
        {
            uint v_Tag;
            while ((v_Tag = p_In.ReadTag()) != 0)
            {
                if (!p_OnField(WireFormat.GetTagFieldNumber(v_Tag), p_In))
                    p_In.SkipLastField();
            }
        }
    }
}
