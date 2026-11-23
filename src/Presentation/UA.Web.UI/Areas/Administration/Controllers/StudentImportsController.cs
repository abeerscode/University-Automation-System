using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Integration;
using UA.Facade.Interface.Student;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("imports.run")]
public sealed class StudentImportsController(IStudentImportFacade students, IExcelWriter csv, IMemoryCache cache, ICurrentUser current) : Controller
{
    private sealed record PendingStudentImport(int OwnerUserId, byte[] Data, string FileName, StudentImportPreview Preview);
    public IActionResult Index() => View();
    [HttpPost, RequestSizeLimit(2_100_000)]
    public async Task<IActionResult> Upload(IFormFile upload)
    {
        if (upload is null || upload.Length is < 1 or > 2_000_000 ||
            Path.GetExtension(upload.FileName).ToLowerInvariant() is not (".csv" or ".xlsx"))
        { TempData["Error"] = "Upload a CSV or .xlsx file (maximum 2 MB)."; return RedirectToAction(nameof(Index)); }
        try
        {
            using var ms = new MemoryStream();
            await upload.CopyToAsync(ms, HttpContext.RequestAborted);
            var bytes = ms.ToArray();
            var report = await students.PreviewAsync(bytes, upload.FileName, HttpContext.RequestAborted);
            var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            if (current.UserId is not int actor) return Forbid();
            cache.Set($"student-import:{actor}:{key}", new PendingStudentImport(actor, bytes, upload.FileName, report),
                TimeSpan.FromMinutes(15));
            ViewBag.Token = key; ViewBag.Report = report;
            return View("Report");
        }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; return RedirectToAction(nameof(Index)); }
    }
    [HttpPost]
    public async Task<IActionResult> Commit(string token)
    {
        if (!TryStage(token, out var stage)) { TempData["Error"] = "Import preview expired. Upload and validate the file again."; return RedirectToAction(nameof(Index)); }
        cache.Remove($"student-import:{current.UserId}:{token}"); // single-use confirmation
        if (stage!.Preview.Issues.Count != 0) return BadRequest("Correct all validation errors before importing.");
        try
        {
            var outcome = await students.CommitAsync(stage.Data, stage.FileName, HttpContext.RequestAborted);
            // Credentials are returned only once over the authorized response; they are never written as seed data.
            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            var rows = outcome.Credentials.Select(c => (IReadOnlyList<string>)new[] { c.StudentId, c.TemporaryPassword });
            return File(csv.WriteCsv(["student_id", "temporary_password"], rows), "text/csv", "student-import-credentials.csv");
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
    [HttpGet]
    public IActionResult ErrorReport(string token)
    {
        if (!TryStage(token, out var stage)) return NotFound();
        var rows = stage!.Preview.Issues.Select(x => (IReadOnlyList<string>)new[] { x.Line.ToString(), x.Message });
        Response.Headers["Cache-Control"] = "no-store";
        return File(csv.WriteCsv(["line", "issue"], rows), "text/csv", "student-import-errors.csv");
    }
    private bool TryStage(string? token, out PendingStudentImport? stage)
    {
        stage = null;
        return current.UserId is int id && !string.IsNullOrWhiteSpace(token) && token.Length == 48 &&
            cache.TryGetValue($"student-import:{id}:{token}", out stage) && stage?.OwnerUserId == id;
    }
    [HttpGet] public IActionResult Template()
    {
        var header = "department_code,admission_year,roll_no,section,full_name,date_of_birth,gender,guardian_name,guardian_phone,phone,address,email\r\n";
        return File(System.Text.Encoding.UTF8.GetBytes(header), "text/csv", "students-import-template.csv");
    }
    [HttpGet] public IActionResult ExcelTemplate()
    {
        string[] headers = ["department_code", "admission_year", "roll_no", "section", "full_name",
            "date_of_birth", "gender", "guardian_name", "guardian_phone", "phone", "address", "email"];
        using var target = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(target, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            static void Entry(System.IO.Compression.ZipArchive zip, string name, string contents)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new System.Text.UTF8Encoding(false));
                writer.Write(contents);
            }
            Entry(archive, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                "</Types>");
            Entry(archive, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"r1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                "</Relationships>");
            Entry(archive, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                "<sheets><sheet name=\"Students\" sheetId=\"1\" r:id=\"r1\"/></sheets></workbook>");
            Entry(archive, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"r1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                "</Relationships>");
            var cells = string.Concat(headers.Select((h,i) =>
                $"<c r=\"{(char)('A' + i)}1\" t=\"inlineStr\"><is><t>{h}</t></is></c>"));
            Entry(archive, "xl/worksheets/sheet1.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                "<sheetData><row r=\"1\">" + cells + "</row></sheetData></worksheet>");
        }
        return File(target.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "students-import-template.xlsx");
    }

}
