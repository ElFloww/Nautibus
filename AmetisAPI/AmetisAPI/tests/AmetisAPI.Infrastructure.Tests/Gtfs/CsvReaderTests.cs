using AmetisAPI.Infrastructure.Gtfs;

namespace AmetisAPI.Infrastructure.Tests.Gtfs
{
    public class CsvReaderTests
    {
        [Fact]
        public void Quoted_Fields_With_Commas_Quotes_And_Newlines_Are_Read_With_Their_Line_Number()
        {
            string v_Csv = "a,b\r\n\"x,1\",\"dit \"\"bonjour\"\"\"\r\n\"multi\nligne\",z\r\nfin,";

            List<(int Line, List<string> Fields)> v_Rows = CsvReader.Read(new StringReader(v_Csv)).ToList();

            Check.That(v_Rows).HasSize(4);
            Check.That(v_Rows[1].Fields).ContainsExactly("x,1", "dit \"bonjour\"");
            Check.That(v_Rows[2].Fields).ContainsExactly("multi\nligne", "z");
            Check.That(v_Rows[3].Line).IsEqualTo(5);
            Check.That(v_Rows[3].Fields).ContainsExactly("fin", "");
        }

        [Fact]
        public void Blank_Lines_Are_Ignored()
        {
            Check.That(CsvReader.Read(new StringReader("a\n\n1\n\n")).Select(p_R => p_R.Fields[0])).ContainsExactly("a", "1");
        }

        [Fact]
        public void Unclosed_Quote_Is_A_Format_Error()
        {
            Check.ThatCode(() => CsvReader.Read(new StringReader("a\n\"ouvert")).ToList()).Throws<FormatException>();
        }
    }
}
