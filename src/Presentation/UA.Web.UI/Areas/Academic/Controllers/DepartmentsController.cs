using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Academic.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Academic.Controllers;
[Area("Academic"), HasPermission("departments.manage")]
public sealed class DepartmentsController(IDepartmentFacade facade, IAcademicCalendarFacade calendar) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.ListAsync());
    public async Task<IActionResult> Edit(int id)
    {
        var d = (await facade.ListAsync()).FirstOrDefault(x => x.Id == id); if (d is null) return NotFound();
        ViewBag.Department = d;
        return View(new DepartmentEditForm { Id = d.Id, Name = d.Name, Status = d.Status });
    }
    [HttpPost] public async Task<IActionResult> Edit(DepartmentEditForm model)
    {
        if (ModelState.IsValid) try
        {
            await facade.UpdateAsync(model.Id, model.Name, model.Status);
            TempData["Message"] = "Department updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Department = (await facade.ListAsync()).FirstOrDefault(x => x.Id == model.Id);
        return View(model);
    }
    [HttpPost] public async Task<IActionResult> Program(ProgramForm model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await facade.UpsertProgramAsync(new ProgramInput(model.DepartmentId, model.Name, model.TotalCredits));
                TempData["Message"] = "Program saved.";
                return RedirectToAction(nameof(Edit), new { id = model.DepartmentId });
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        }
        else TempData["Error"] = "Invalid program details.";
        return RedirectToAction(nameof(Edit), new { id = model.DepartmentId });
    }
    [HasPermission("config.manage")]
    public async Task<IActionResult> Settings(int departmentId, int termId = 0)
    {
        var terms = await calendar.TermsAsync();
        if (termId == 0) termId = terms.FirstOrDefault()?.Id ?? 0;
        if (termId == 0) return RedirectToAction("Index", "AcademicCalendar");
        var data = await facade.SettingsAsync(departmentId, termId);
        ViewBag.Terms = terms;
        return View(new SettingsForm { DepartmentId = data.DepartmentId, TermId = data.TermId,
            NormalCredits = data.NormalCredits, MinimumAttendance = data.MinimumAttendance,
            ClassStarts = data.ClassStarts, ClassEnds = data.ClassEnds,
            SeatsPerIntake = data.SeatsPerIntake, SectionCapacity = data.SectionCapacity });
    }
    [HttpPost, HasPermission("config.manage")]
    public async Task<IActionResult> Settings(SettingsForm model)
    {
        if (ModelState.IsValid) try
        {
            await facade.SaveSettingsAsync(new DepartmentSettingsInput(model.DepartmentId, model.TermId,
                model.NormalCredits, model.MinimumAttendance, model.ClassStarts, model.ClassEnds,
                model.SeatsPerIntake, model.SectionCapacity));
            TempData["Message"] = "Versioned academic rules saved.";
            return RedirectToAction(nameof(Settings), new { departmentId = model.DepartmentId, termId = model.TermId });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Terms = await calendar.TermsAsync(); return View(model);
    }
}
