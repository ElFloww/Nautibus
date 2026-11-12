using AmetisAPI.Domain.Bo.Gtfs;
using AmetisAPI.Domain.Exceptions;

namespace AmetisAPI.Infrastructure.Gtfs
{
    /// <summary>Un fichier GTFS : contrôle des colonnes obligatoires et conversion ligne à ligne.</summary>
    internal sealed class GtfsTable
    {
        private readonly string m_FileName;
        private readonly List<(int Line, List<string> Fields)> m_Rows;
        private readonly Dictionary<string, int> m_Columns;

        public GtfsTable(string p_FileName, TextReader p_Reader, params string[] p_RequiredColumns)
        {
            m_FileName = p_FileName;
            List<(int Line, List<string> Fields)> v_All;
            try
            {
                v_All = CsvReader.Read(p_Reader).ToList();
            }
            catch (FormatException v_Ex)
            {
                throw new GtfsFormatException($"{p_FileName} : {v_Ex.Message}");
            }
            if (v_All.Count == 0)
                throw new GtfsFormatException($"{p_FileName} : fichier vide");

            m_Columns = v_All[0].Fields
                .Select((p_Name, p_Index) => (Name: p_Name.Trim().TrimStart('﻿'), Index: p_Index))
                .GroupBy(p_C => p_C.Name)
                .ToDictionary(p_G => p_G.Key, p_G => p_G.First().Index);
            string v_Missing = p_RequiredColumns.FirstOrDefault(p_C => !m_Columns.ContainsKey(p_C));
            if (v_Missing is not null)
                throw new GtfsFormatException($"{p_FileName} : colonne obligatoire « {v_Missing} » absente");
            m_Rows = v_All.Skip(1).ToList();
        }

        public bool HasColumn(string p_Column) => m_Columns.ContainsKey(p_Column);

        /// <summary>Convertit chaque ligne ; une ligne invalide est rejetée avec sa raison et son numéro.</summary>
        public List<T> Map<T>(Func<Row, T> p_Map, List<GtfsRejection> p_Rejections)
        {
            List<T> v_Result = new(m_Rows.Count);
            foreach ((int v_Line, List<string> v_Fields) in m_Rows)
            {
                if (v_Fields.Count > m_Columns.Count)
                {
                    p_Rejections.Add(new GtfsRejection(m_FileName, v_Line, $"{v_Fields.Count} champs pour {m_Columns.Count} colonnes"));
                    continue;
                }
                try
                {
                    T v_Item = p_Map(new Row(m_Columns, v_Fields));
                    if (v_Item is not null)
                        v_Result.Add(v_Item);
                }
                catch (FormatException v_Ex)
                {
                    p_Rejections.Add(new GtfsRejection(m_FileName, v_Line, v_Ex.Message));
                }
            }
            return v_Result;
        }

        internal readonly struct Row(Dictionary<string, int> p_Columns, List<string> p_Fields)
        {
            /// <summary>Valeur brute (sans espaces de bord), null si vide ou colonne absente.</summary>
            public string Get(string p_Column)
            {
                if (!p_Columns.TryGetValue(p_Column, out int v_Index) || v_Index >= p_Fields.Count)
                    return null;
                string v_Value = p_Fields[v_Index].Trim();
                return v_Value.Length == 0 ? null : v_Value;
            }

            public string Required(string p_Column) => Get(p_Column) ?? throw new FormatException($"{p_Column} vide");

            public int? Int(string p_Column) => Get(p_Column) is { } v_V
                ? int.TryParse(v_V, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int v_I) ? v_I : throw new FormatException($"{p_Column} « {v_V} » n'est pas un entier")
                : null;

            public double? Double(string p_Column) => Get(p_Column) is { } v_V
                ? double.TryParse(v_V, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double v_D) ? v_D : throw new FormatException($"{p_Column} « {v_V} » n'est pas un nombre")
                : null;

            public DateOnly Date(string p_Column) => DateOnly.TryParseExact(Required(p_Column), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateOnly v_D)
                ? v_D
                : throw new FormatException($"{p_Column} « {Get(p_Column)} » n'est pas une date AAAAMMJJ");

            /// <summary>Heure GTFS H:MM:SS (peut dépasser 24 h) en secondes ; null si vide.</summary>
            public int? Time(string p_Column)
            {
                string v_Value = Get(p_Column);
                if (v_Value is null)
                    return null;
                string[] v_Parts = v_Value.Split(':');
                if (v_Parts.Length == 3
                    && int.TryParse(v_Parts[0], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int v_H)
                    && v_Parts[1].Length == 2 && int.TryParse(v_Parts[1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int v_M) && v_M < 60
                    && v_Parts[2].Length == 2 && int.TryParse(v_Parts[2], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int v_S) && v_S < 60)
                    return v_H * 3600 + v_M * 60 + v_S;
                throw new FormatException($"{p_Column} « {v_Value} » n'est pas une heure H:MM:SS");
            }
        }
    }
}
