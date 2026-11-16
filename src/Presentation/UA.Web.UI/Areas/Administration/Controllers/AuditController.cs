using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("audit.view")]
public sealed class AuditController(IAuditFacade facade) : Controller
{
    [HasPermission("exports.sensitive")]
    public async Task<IActionResult> Export(string? entity = null)
    {
        var bytes = await facade.ExportCsvAsync(entity);
        return File(bytes, "text/csv", "audit-log.csv");
    }
    public async Task<IActionResult> Index(string? entity = null)
    { ViewBag.Entity = entity; return View(await facade.SearchAsync(entity)); }
}
