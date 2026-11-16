using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("config.manage")]
public sealed class GradeScalesController(IAcademicRulesFacade facade) : Controller
{
    public async Task<IActionResult> Index(int departmentId = 1)
    { ViewBag.DepartmentId = departmentId; return View(await facade.GradeBandsAsync(departmentId)); }
    [HttpGet] public IActionResult Edit(int departmentId = 1) => View(new RuleVersionForm { DepartmentId = departmentId,
        Rows = "F,0,40,0\nD,40,45,2\nC,45,50,2.25\nC+,50,55,2.5\nB-,55,60,2.75\nB,60,65,3\nB+,65,70,3.25\nA-,70,75,3.5\nA,75,80,3.75\nA+,80,100,4" });
    [HttpPost] public async Task<IActionResult> Edit(RuleVersionForm form)
    {
        if (!ModelState.IsValid) return View(form);
        try
        {
            var rows = form.Rows.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split(','))
                .Select(p => new GradeBandDto(p[0].Trim(), decimal.Parse(p[1], CultureInfo.InvariantCulture),
                    decimal.Parse(p[2], CultureInfo.InvariantCulture), decimal.Parse(p[3], CultureInfo.InvariantCulture), 0)).ToList();
            int version = await facade.AddGradeVersionAsync(form.DepartmentId, form.EffectiveFrom, rows);
            TempData["Message"] = "Grade scale version " + version + " created.";
            return RedirectToAction(nameof(Index), new { departmentId = form.DepartmentId });
        }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or BusinessRuleException or OverflowException)
        { ModelState.AddModelError("", ex.Message); return View(form); }
    }
}
