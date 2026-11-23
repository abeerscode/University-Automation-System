using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Academic.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Academic.Controllers;
[Area("Academic"), HasPermission("batches.manage")]
public sealed class BatchesController(IBatchFacade facade, IDepartmentFacade departments) : Controller
{
    public async Task<IActionResult> Index(int? departmentId) => View(await facade.ListAsync(departmentId));
    public async Task<IActionResult> Details(int id)
    {
        var b = (await facade.ListAsync()).FirstOrDefault(x => x.Id == id);
        return b is null ? NotFound() : View(b);
    }
    public async Task<IActionResult> Create()
    { ViewBag.Departments = await departments.AvailableAsync("batches.manage"); return View(new BatchForm()); }
    [HttpPost] public async Task<IActionResult> Create(BatchForm form)
    {
        if (ModelState.IsValid) try
        {
            int id = await facade.CreateAsync(new BatchInput(form.DepartmentId, form.Year, form.Seats, form.SectionCapacity));
            TempData["Message"] = "Batch and sections created."; return RedirectToAction(nameof(Details), new { id });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Departments = await departments.AvailableAsync("batches.manage"); return View(form);
    }
    [HttpPost] public async Task<IActionResult> AddSection(SectionForm form)
    {
        try { await facade.AddSectionAsync(form.BatchId, form.Name, form.Capacity); TempData["Message"] = "Section added."; }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id = form.BatchId });
    }
}
