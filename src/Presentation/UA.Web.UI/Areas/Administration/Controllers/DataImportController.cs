using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("imports.run")]
public sealed class DataImportController(IImportFacade facade) : Controller
{
    public IActionResult Index() => View();
    [HttpPost, RequestSizeLimit(2_000_000)]
    public async Task<IActionResult> Upload(IFormFile upload)
    {
        if (upload is null || upload.Length == 0 || upload.Length > 2_000_000 ||
            !string.Equals(Path.GetExtension(upload.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
        { TempData["Error"] = "Upload a non-empty CSV file (maximum 2 MB)."; return RedirectToAction(nameof(Index)); }
        using var stream = upload.OpenReadStream();
        ViewBag.Report = await facade.ValidateCsvAsync(stream, ["student_id", "name", "department_code"]);
        return View("Report");
    }
    public IActionResult Template()
    { return File(System.Text.Encoding.UTF8.GetBytes("student_id,name,department_code\r\n"), "text/csv", "students-template.csv"); }
}
