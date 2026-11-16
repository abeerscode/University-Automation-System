using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("config.manage")]
public sealed class AttendanceScalesController(IAcademicRulesFacade facade) : Controller
{
    public async Task<IActionResult> Index(int departmentId = 1)
    { ViewBag.DepartmentId = departmentId; return View(await facade.AttendanceBandsAsync(departmentId)); }
    [HttpGet] public IActionResult Edit(int departmentId = 1) => View(new RuleVersionForm { DepartmentId = departmentId,
        Rows = "0,0\n70,20\n75,40\n80,60\n85,80\n90,100" });
    [HttpPost] public async Task<IActionResult> Edit(RuleVersionForm form)
    {
        if (!ModelState.IsValid) return View(form);
        try
        {
            var rows = form.Rows.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split(','))
                .Select(p => new AttendanceBandDto(decimal.Parse(p[0], CultureInfo.InvariantCulture),
                    decimal.Parse(p[1], CultureInfo.InvariantCulture), 0)).ToList();
            int version = await facade.AddAttendanceVersionAsync(form.DepartmentId, form.EffectiveFrom, rows);
            TempData["Message"] = "Attendance scale version " + version + " created.";
            return RedirectToAction(nameof(Index), new { departmentId = form.DepartmentId });
        }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or BusinessRuleException or OverflowException)
        { ModelState.AddModelError("", ex.Message); return View(form); }
    }
}
