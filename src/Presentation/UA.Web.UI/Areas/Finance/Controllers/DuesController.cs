using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Finance;
using UA.Framework.Exceptions;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Finance.Controllers;
[Area("Finance")]
public sealed class DuesController(IDueFacade dues) : Controller
{
    [HttpGet, HasPermission("dues.view")] public async Task<IActionResult> Index(int? departmentId,int? batchId,int? sectionId,string? status) { ViewBag.Lookups=await dues.LookupAsync("dues.view"); ViewBag.DepartmentId=departmentId; ViewBag.BatchId=batchId; ViewBag.SectionId=sectionId; ViewBag.Status=status; return View(await dues.DuesAsync(departmentId,batchId,sectionId,status)); }
    [HttpPost, HasPermission("fees.manage")] public async Task<IActionResult> Generate(string studentId,int termId) { if(!ModelState.IsValid || string.IsNullOrWhiteSpace(studentId)) { TempData["Error"]="Student ID and term are required."; return RedirectToAction("Index","FeeTypes"); } try { await dues.GenerateAsync(studentId.Trim(),termId); TempData["Message"]="Eligible semester dues generated (existing dues were retained)."; } catch(BusinessRuleException ex) { TempData["Error"]=ex.Message; } return RedirectToAction("Index","FeeTypes"); }
}
