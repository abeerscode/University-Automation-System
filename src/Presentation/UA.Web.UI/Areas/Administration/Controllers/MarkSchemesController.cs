using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("config.manage")]
public sealed class MarkSchemesController(IAcademicRulesFacade facade) : Controller
{
    public async Task<IActionResult> Index(int departmentId = 1, string courseType = "Theory")
    { ViewBag.DepartmentId = departmentId; ViewBag.CourseType = courseType; return View(await facade.MarkPartsAsync(departmentId, courseType)); }
    [HttpGet] public IActionResult Edit(int departmentId = 1, string courseType = "Theory")
        => View(new RuleVersionForm { DepartmentId = departmentId, CourseType = courseType,
            Rows = courseType == "Theory" ? "Final,50,false\nMidterm,25,false\nAttendance,5,true\nTests and Assignments,20,false" : "Attendance,20,true\nReport,30,false\nProject,50,false" });
    [HttpPost] public async Task<IActionResult> Edit(RuleVersionForm form)
    {
        if (!ModelState.IsValid) return View(form);
        try
        {
            var rows = form.Rows.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split(','))
                .Select(p => new MarkPartDto(form.CourseType, p[0].Trim(), decimal.Parse(p[1], CultureInfo.InvariantCulture),
                    bool.Parse(p[2]), 0)).ToList();
            int version = await facade.AddMarkSchemeVersionAsync(form.DepartmentId, form.EffectiveFrom, form.CourseType, rows);
            TempData["Message"] = "Mark scheme version " + version + " created.";
            return RedirectToAction(nameof(Index), new { departmentId = form.DepartmentId, courseType = form.CourseType });
        }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or BusinessRuleException or OverflowException)
        { ModelState.AddModelError("", ex.Message); return View(form); }
    }
}
