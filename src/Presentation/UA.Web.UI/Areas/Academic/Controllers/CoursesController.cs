using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Academic.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Academic.Controllers;
[Area("Academic"), HasPermission("courses.manage")]
public sealed class CoursesController(ICourseFacade facade, IDepartmentFacade departments) : Controller
{
    public async Task<IActionResult> Index(int? departmentId) => View(await facade.ListAsync(departmentId));
    public async Task<IActionResult> Create(int? departmentId)
    {
        int id = departmentId ?? (await departments.AvailableAsync("courses.manage")).FirstOrDefault()?.Id ?? 0;
        if (id == 0) return RedirectToAction("Index", "Departments");
        await Prepare(id, 0); return View("Form", new CourseForm { DepartmentId = id });
    }
    public async Task<IActionResult> Edit(int id)
    {
        var c = (await facade.ListAsync()).FirstOrDefault(x => x.Id == id); if (c is null) return NotFound();
        await Prepare(c.DepartmentId, id);
        return View("Form", new CourseForm { Id = c.Id, DepartmentId = c.DepartmentId, Code = c.Code,
            Title = c.Title, Credits = c.Credits, Type = c.Type, PrerequisiteIds = c.Prerequisites.ToArray() });
    }
    private async Task Prepare(int deptId, int excluded)
        => ViewBag.Candidates = (await facade.ListAsync(deptId)).Where(x => x.Id != excluded).ToArray();
    [HttpPost] public async Task<IActionResult> Save(CourseForm form)
    {
        if (ModelState.IsValid) try
        {
            await facade.SaveAsync(new CourseInput(form.Id, form.DepartmentId, form.Code, form.Title,
                form.Credits, form.Type, form.PrerequisiteIds ?? []));
            TempData["Message"] = "Course saved."; return RedirectToAction(nameof(Index), new { departmentId = form.DepartmentId });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        await Prepare(form.DepartmentId, form.Id); return View("Form", form);
    }
}
