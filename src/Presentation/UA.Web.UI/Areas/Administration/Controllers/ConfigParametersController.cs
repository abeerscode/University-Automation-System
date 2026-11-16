using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("config.manage")]
public sealed class ConfigParametersController(IConfigurationFacade facade) : Controller
{
    public async Task<IActionResult> Index(string? key = null)
    { ViewBag.Key = key; return View(await facade.HistoryAsync(key)); }
    [HttpGet] public IActionResult Edit() => View(new ConfigFormViewModel());
    [HttpPost] public async Task<IActionResult> Edit(ConfigFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        await facade.SetAsync(model.Key, model.Value, model.DepartmentId, model.EffectiveTermId);
        TempData["Message"] = "A new configuration version was created.";
        return RedirectToAction(nameof(Index), new { key = model.Key });
    }
}
