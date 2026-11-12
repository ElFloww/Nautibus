using System.IO.Compression;
using System.Text;

namespace AmetisAPI.Infrastructure.Tests.Gtfs
{
    /// <summary>Archive GTFS de test construite en mémoire ; chaque fichier peut être remplacé ou retiré.</summary>
    internal static class GtfsZipFixture
    {
        public static Dictionary<string, string> Files() => new()
        {
            ["agency.txt"] = "﻿agency_id,agency_name,agency_url,agency_timezone\nametis,Keolis Amiens,https://www.ametis.fr/,Europe/Paris\n",
            ["routes.txt"] = "route_id,agency_id,route_short_name,route_long_name,route_type,route_color,route_text_color,route_sort_order\nL1,ametis,1,\"Gare <> CHU, Sud\",3,E3C305,17202A,1\n",
            ["stops.txt"] = "stop_id,stop_name,stop_lat,stop_lon,location_type,parent_station\nAREA,Gare,49.89,2.30,1,\nQ1,Gare,49.8901,2.3001,0,AREA\nQ2,CHU,49.87,2.25,,\n",
            ["trips.txt"] = "route_id,service_id,trip_id,trip_headsign,direction_id,shape_id\nL1,SEM,T1,Aller,0,S1\n",
            ["stop_times.txt"] = "trip_id,arrival_time,departure_time,stop_id,location_id,stop_sequence,pickup_type,drop_off_type\nT1,8:00:00,8:00:00,Q1,,1,0,1\nT1,24:37:00,24:37:00,Q2,,2,1,0\nT1,,,,ZONE,3,0,0\nT1,9h00,9h00,Q2,,4,0,0\n",
            ["calendar.txt"] = "service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date\nSEM,1,1,1,1,1,0,0,20261101,20270501\n",
            ["calendar_dates.txt"] = "service_id,date,exception_type\nSEM,20261111,2\n",
            ["shapes.txt"] = "shape_id,shape_pt_lat,shape_pt_lon,shape_pt_sequence\nS1,49.89,2.30,0\nS1,49.87,2.25,1\n"
        };

        public static MemoryStream Zip(Dictionary<string, string> p_Files)
        {
            MemoryStream v_Stream = new();
            using (ZipArchive v_Zip = new(v_Stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach ((string v_Name, string v_Content) in p_Files)
                {
                    using StreamWriter v_Writer = new(v_Zip.CreateEntry(v_Name).Open(), new UTF8Encoding(false));
                    v_Writer.Write(v_Content);
                }
            }
            v_Stream.Position = 0;
            return v_Stream;
        }
    }
}
