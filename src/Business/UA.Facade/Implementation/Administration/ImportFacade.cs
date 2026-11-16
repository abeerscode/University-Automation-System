using Microsoft.VisualBasic.FileIO;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
namespace UA.Facade.Implementation.Administration;
public sealed class ImportFacade(IUserAccessFacade access) : IImportFacade
{
    public async Task<ImportValidationDto> ValidateCsvAsync(Stream input, IReadOnlyList<string> requiredHeaders, CancellationToken ct = default)
    {
        await access.RequireAsync("imports.run", ct: ct);
        using var reader = new StreamReader(input, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        using var parser = new TextFieldParser(reader) { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true };
        parser.SetDelimiters(",");
        var issues = new List<ImportIssue>(); int valid = 0, line = 1;
        try
        {
            if (parser.EndOfData) return new(0, [new(1, "File is empty.")]);
            var headers = parser.ReadFields() ?? [];
            var missing = requiredHeaders.Except(headers, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var h in missing) issues.Add(new(1, $"Missing column: {h}"));
            if (missing.Count > 0) return new(0, issues);
            while (!parser.EndOfData && line < 10001)
            {
                ct.ThrowIfCancellationRequested(); line++;
                var cells = parser.ReadFields() ?? [];
                if (cells.Length != headers.Length) issues.Add(new(line, "Wrong number of columns."));
                else if (cells.All(string.IsNullOrWhiteSpace)) issues.Add(new(line, "Empty row."));
                else valid++;
            }
            if (!parser.EndOfData) issues.Add(new(line, "Preview is limited to 10,000 rows."));
        }
        catch (MalformedLineException ex) { issues.Add(new(line, "Malformed CSV: " + ex.Message)); }
        return new(valid, issues);
    }
}
