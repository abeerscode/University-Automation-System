using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
using UA.Framework.Constants;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("roles.manage")]
public sealed class RolesController(IRoleFacade facade) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.ListAsync());
    [HttpGet] public IActionResult Create() => View(new RoleFormViewModel());
    [HttpPost] public async Task<IActionResult> Create(RoleFormViewModel model)
    { if (!ModelState.IsValid) return View(model); int id = await facade.CreateAsync(model.Name, model.DepartmentScoped); return RedirectToAction(nameof(Permissions), new { id }); }
    [HttpGet] public async Task<IActionResult> Permissions(int id)
    {
        ViewBag.Catalog = PermissionCodes.All;
        ViewBag.Selected = (await facade.PermissionsAsync(id)).ToHashSet();
        ViewBag.RoleId = id;
        return View();
    }
    [HttpPost] public async Task<IActionResult> Permissions(int id, string[]? permissions)
    { await facade.SetPermissionsAsync(id, permissions ?? []); TempData["Message"] = "Permissions updated."; return RedirectToAction(nameof(Permissions), new { id }); }
}
