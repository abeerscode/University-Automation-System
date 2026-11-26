using System.Text;
using System.Globalization;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Integration;
using UA.Framework.Exceptions;
namespace UA.Integration.Import;
/// <summary>Reads a single-sheet .xlsx or RFC-4180-style .csv without adding new NuGet packages.</summary>
public sealed class StudentImportReader : IStudentImportReader
{
    public StudentImportSheet Read(byte[] data, string extension)
    {
        if (data.Length is < 1 or > 2_000_000)
            throw new BusinessRuleException("Import must be nonempty and at most 2 MB.");
        return extension.Trim().ToLowerInvariant() switch
        {
            ".csv" => ReadCsv(data),
            ".xlsx" => ReadXlsx(data),
            _ => throw new BusinessRuleException("Only CSV and Excel (.xlsx) files are accepted.")
        };
    }
    private static StudentImportSheet ReadCsv(byte[] data)
    {
        try
        {
            using var stream = new MemoryStream(data, writable: false);
            using var reader = new StreamReader(stream, new System.Text.UTF8Encoding(false, true),
                detectEncodingFromByteOrderMarks: true);
            using var parser = new TextFieldParser(reader) { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true };
            parser.SetDelimiters(",");
            if (parser.EndOfData) throw new BusinessRuleException("The file has no header row.");
            var headers = (parser.ReadFields() ?? []).Select(x => x.Trim().TrimStart('\uFEFF')).ToArray();
            var rows = new List<StudentImportRawRow>();
            int line = 1;
            while (!parser.EndOfData)
            {
                if (++line > 5001) throw new BusinessRuleException("Maximum 5,000 student rows per import.");
                var values = parser.ReadFields() ?? [];
                var map = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < headers.Length; c++) map[headers[c]] = c < values.Length ? values[c].Trim() : "";
                if (values.Length != headers.Length) map["__error"] = "Column count differs from the header.";
                rows.Add(new StudentImportRawRow(line, map));
            }
            return new StudentImportSheet(headers, rows);
        }
        catch (MalformedLineException ex) { throw new BusinessRuleException("Malformed CSV row: " + ex.Message); }
        catch (DecoderFallbackException) { throw new BusinessRuleException("CSV must be UTF-8 encoded."); }
    }
    private static StudentImportSheet ReadXlsx(byte[] data)
    {
        try
        {
            using var input = new MemoryStream(data, writable: false);
            using var zip = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: false);
            if (zip.Entries.Count > 250) throw new BusinessRuleException("Workbook has too many package entries.");
            var strings = new List<string>();
            var ss = zip.GetEntry("xl/sharedStrings.xml");
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            if (ss is not null)
            {
                var doc = Xml(ss);
                strings.AddRange(doc.Descendants(ns + "si").Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value))));
            }
            var sheet = zip.Entries.Where(x => x.FullName.StartsWith("xl/worksheets/sheet", StringComparison.Ordinal) &&
                x.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.FullName, StringComparer.Ordinal).FirstOrDefault()
                ?? throw new BusinessRuleException("Workbook has no worksheet.");
            var worksheet = Xml(sheet);
            var grid = new List<(int Number, Dictionary<int,string> Cells)>();
            int seen = 0;
            foreach (var row in worksheet.Descendants(ns + "sheetData").Elements(ns + "row"))
            {
                if (++seen > 5001) throw new BusinessRuleException("Maximum 5,000 student rows per import.");
                int rowNumber = int.TryParse(row.Attribute("r")?.Value, out int n) ? n : seen;
                var values = new Dictionary<int,string>(); int sequential = 0;
                foreach (var cell in row.Elements(ns + "c"))
                {
                    if (cell.Element(ns + "f") is not null)
                        throw new BusinessRuleException("Formula cells are not supported. Paste values before importing.");
                    int column = ColumnIndex(cell.Attribute("r")?.Value) ?? sequential;
                    sequential = column + 1;
                    string type = cell.Attribute("t")?.Value ?? "";
                    string content = type == "inlineStr" ? string.Concat(cell.Descendants(ns + "t").Select(t => t.Value))
                        : cell.Element(ns + "v")?.Value ?? "";
                    if (type == "s")
                    {
                        if (!int.TryParse(content, NumberStyles.None, CultureInfo.InvariantCulture, out int index) || index < 0 || index >= strings.Count)
                            throw new BusinessRuleException("Invalid Excel shared-string reference.");
                        content = strings[index];
                    }
                    values[column] = content.Trim();
                }
                if (values.Count > 0) grid.Add((rowNumber, values));
            }
            if (grid.Count == 0) throw new BusinessRuleException("Worksheet has no header row.");
            var headerCells = grid[0].Cells;
            int maximum = headerCells.Keys.Max();
            if (maximum > 100) throw new BusinessRuleException("Too many columns in import template.");
            string[] headers = Enumerable.Range(0, maximum + 1).Select(i => headerCells.GetValueOrDefault(i, "").Trim()).ToArray();
            var rows = grid.Skip(1).Select(row =>
            {
                var values = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
                for (int i=0; i<headers.Length; i++)
                    if (!string.IsNullOrEmpty(headers[i])) values[headers[i]] = row.Cells.GetValueOrDefault(i, "");
                return new StudentImportRawRow(row.Number, values);
            }).ToArray();
            return new StudentImportSheet(headers, rows);
        }
        catch (InvalidDataException) { throw new BusinessRuleException("Invalid Excel workbook."); }
        catch (XmlException) { throw new BusinessRuleException("Malformed Excel worksheet XML."); }
    }
    private static XDocument Xml(ZipArchiveEntry entry)
    {
        if (entry.Length > 8_000_000) throw new BusinessRuleException("Workbook worksheet exceeds the safety limit.");
        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
            { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 8_000_000 });
        return XDocument.Load(reader);
    }
    private static int? ColumnIndex(string? reference)
    {
        if (string.IsNullOrEmpty(reference)) return null;
        int value = 0;
        foreach (char ch in reference)
        {
            if (ch is >= 'A' and <= 'Z') value = value * 26 + (ch - 'A' + 1);
            else if (ch is >= 'a' and <= 'z') value = value * 26 + (ch - 'a' + 1);
            else break;
        }
        return value == 0 ? null : value - 1;
    }
}
