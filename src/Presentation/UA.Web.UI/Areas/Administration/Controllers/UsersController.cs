using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("users.view")]
public sealed class UsersController(IUserFacade facade, IRoleFacade roles, IUserAccessFacade access) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.ListAsync());
    public async Task<IActionResult> Details(int id)
    {
        var user = await facade.GetAsync(id); if (user is null) return NotFound();
        ViewBag.Roles = (await access.ForUserAsync(id)).Assignments;
        ViewBag.RoleChoices = await roles.ListAsync();
        return View(user);
    }
    [HasPermission("users.manage"), HttpGet]
    public async Task<IActionResult> Create()
    { ViewBag.Roles = await roles.ListAsync(); return View(new UserFormViewModel()); }
    [HasPermission("users.manage"), HttpPost]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (!ModelState.IsValid) { ViewBag.Roles = await roles.ListAsync(); return View(model); }
        try
        {
            int id = await facade.CreateAsync(new UserCreateDto(model.Username, model.Password, model.Email,
                model.Mobile, model.RoleId, model.DepartmentId));
            TempData["Message"] = "Account created. Ask the user to verify their email.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BusinessRuleException ex)
        { ModelState.AddModelError("", ex.Message); ViewBag.Roles = await roles.ListAsync(); return View(model); }
    }
    [HasPermission("users.manage"), HttpPost]
    public async Task<IActionResult> Status(int id, bool active)
    { await facade.SetStatusAsync(id, active); return RedirectToAction(nameof(Details), new { id }); }
    [HasPermission("users.manage"), HttpPost]
    public async Task<IActionResult> Assign(int id, int roleId, int? departmentId)
    { await facade.AssignRoleAsync(id, roleId, departmentId); return RedirectToAction(nameof(Details), new { id }); }
    [HasPermission("users.manage"), HttpPost]
    public async Task<IActionResult> RemoveRole(int id, int roleId)
    { await facade.RemoveRoleAsync(id, roleId); return RedirectToAction(nameof(Details), new { id }); }
}
