using System.Text;
using UA.Facade.Interface.Integration;
namespace UA.Integration.Reporting;
public sealed class CsvWriter : IExcelWriter
{
    public byte[] WriteCsv(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        static string Cell(string s)
        {
            // Prevent spreadsheet formula execution when importing CSV into Excel.
            if (s.Length > 0 && "=+-@".Contains(s[0])) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        var sb = new StringBuilder(); sb.AppendLine(string.Join(",", headers.Select(Cell)));
        foreach (var row in rows) sb.AppendLine(string.Join(",", row.Select(Cell)));
        return new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }
}
