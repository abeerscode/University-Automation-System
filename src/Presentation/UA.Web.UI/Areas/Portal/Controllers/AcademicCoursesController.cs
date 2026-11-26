using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
namespace UA.Web.UI.Areas.Portal.Controllers;
[Area("Portal")]
public sealed class AcademicCoursesController(IAcademicOperationsFacade facade) : Controller
{
    public async Task<IActionResult> Index()=>View(await facade.OwnCoursesAsync(HttpContext.RequestAborted));
    public async Task<IActionResult> Teaching(int id)=>View(await facade.DetailAsync(id,true,HttpContext.RequestAborted));
    public async Task<IActionResult> Session(int id,int routineId,DateTime date)
    {if(!ModelState.IsValid)return BadRequest();return View(await facade.SessionAsync(id,routineId,date,HttpContext.RequestAborted));}
    [HttpPost] public async Task<IActionResult> Attendance(int id,int routineId,DateTime date,int[]? presentIds,int[]? enrollmentIds)
    {if(!ModelState.IsValid){TempData["Error"]="Invalid attendance input.";return RedirectToAction(nameof(Teaching),new{id});}try{await facade.RecordAttendanceAsync(id,routineId,date,presentIds??[],enrollmentIds??[],HttpContext.RequestAborted);TempData["Message"]="Attendance saved.";}catch(BusinessRuleException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Teaching),new{id});}
}
