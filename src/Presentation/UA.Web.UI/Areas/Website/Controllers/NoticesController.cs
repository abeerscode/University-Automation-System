using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Website;
using UA.Web.UI.Areas.Website.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Website.Controllers;
[Area("Website"), HasPermission("notices.edit")]
public sealed class NoticesController(INoticeFacade facade,Microsoft.AspNetCore.OutputCaching.IOutputCacheStore cache) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.ListAsync());
    [HttpGet] public IActionResult Create() => View(new NoticeFormViewModel());
    [HttpPost] public async Task<IActionResult> Create(NoticeFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        await facade.DraftAsync(model.DepartmentId, model.Title, model.Body, model.IsPublic);
        TempData["Message"] = "Notice saved for independent approval.";
        return RedirectToAction(nameof(Index));
    }
    [HasPermission("notices.approve"), HttpPost]
    public async Task<IActionResult> Approve(int id)
    { await facade.ApproveAsync(id); await cache.EvictByTagAsync("public-site",HttpContext.RequestAborted); return RedirectToAction(nameof(Index)); }
}
