using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("approvals.record")]
public sealed class ApprovalsController(IApprovalFacade facade) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.ListAsync());
    [HttpGet] public IActionResult Create() => View(new ApprovalFormViewModel());
    [HttpPost] public async Task<IActionResult> Create(ApprovalFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var record = await facade.RecordAsync(model.Type, model.Status, model.DepartmentId, model.RequestorId, model.ScanPath);
        TempData["Message"] = "Approval recorded under " + record.Reference;
        return RedirectToAction(nameof(Index));
    }
    [HasPermission("approvals.correct"), HttpGet]
    public IActionResult Correct(int id) { ViewBag.ApprovalId = id; return View(); }
    [HasPermission("approvals.correct"), HttpPost]
    public async Task<IActionResult> Correct(int id, string status, string? scanPath)
    {
        var updated = await facade.CorrectAsync(id, status, scanPath);
        TempData["Message"] = "Superseding approval created: " + updated.Reference;
        return RedirectToAction(nameof(Index));
    }
}
