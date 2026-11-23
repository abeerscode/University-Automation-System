using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Academic.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Academic.Controllers;
[Area("Academic"), HasPermission("rooms.manage")]
public sealed class RoomsController(IRoomFacade facade) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.ListAsync());
    public IActionResult Create() => View("Form", new RoomForm());
    public async Task<IActionResult> Edit(int id)
    {
        var r = (await facade.ListAsync()).FirstOrDefault(x => x.Id == id); if (r is null) return NotFound();
        return View("Form", new RoomForm { Id = r.Id, Name = r.Name, Capacity = r.Capacity, Type = r.Type });
    }
    [HttpPost] public async Task<IActionResult> Save(RoomForm form)
    {
        if (ModelState.IsValid) try
        {
            await facade.SaveAsync(new RoomInput(form.Id, form.Name, form.Capacity, form.Type));
            TempData["Message"] = "Room saved."; return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        return View("Form", form);
    }
}
