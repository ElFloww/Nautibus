using System.Text;

namespace AmetisAPI.Infrastructure.Gtfs
{
    /// <summary>Lecteur CSV RFC 4180 (guillemets, guillemets doublés, retours à la ligne dans un champ).</summary>
    internal static class CsvReader
    {
        /// <summary>Retourne chaque enregistrement avec le numéro de sa première ligne (1 = en-tête).</summary>
        public static IEnumerable<(int Line, List<string> Fields)> Read(TextReader p_Reader)
        {
            List<string> v_Fields = [];
            StringBuilder v_Field = new();
            bool v_InQuotes = false;
            bool v_HasData = false;
            int v_Line = 1;
            int v_RecordLine = 1;
            int v_Char;

            while ((v_Char = p_Reader.Read()) != -1)
            {
                char v_C = (char)v_Char;
                if (v_InQuotes)
                {
                    if (v_C == '"')
                    {
                        if (p_Reader.Peek() == '"')
                        {
                            v_Field.Append('"');
                            p_Reader.Read();
                        }
                        else
                        {
                            v_InQuotes = false;
                        }
                    }
                    else
                    {
                        if (v_C == '\n')
                            v_Line++;
                        v_Field.Append(v_C);
                    }
                    continue;
                }

                switch (v_C)
                {
                    case '"':
                        v_InQuotes = true;
                        v_HasData = true;
                        break;
                    case ',':
                        v_Fields.Add(v_Field.ToString());
                        v_Field.Clear();
                        v_HasData = true;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        if (v_HasData || v_Field.Length > 0)
                        {
                            v_Fields.Add(v_Field.ToString());
                            yield return (v_RecordLine, v_Fields);
                        }
                        v_Fields = [];
                        v_Field.Clear();
                        v_HasData = false;
                        v_Line++;
                        v_RecordLine = v_Line;
                        break;
                    default:
                        v_Field.Append(v_C);
                        v_HasData = true;
                        break;
                }
            }

            if (v_InQuotes)
                throw new FormatException($"Guillemet non fermé à partir de la ligne {v_RecordLine}");
            if (v_HasData || v_Field.Length > 0)
            {
                v_Fields.Add(v_Field.ToString());
                yield return (v_RecordLine, v_Fields);
            }
        }
    }
}
