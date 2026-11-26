using UA.Facade.Interface.Integration;
using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
namespace UA.Web.UI.Areas.Academic.Controllers;
[Area("Academic")]
public sealed class OperationsController(IAcademicOperationsFacade facade, IExcelWriter writer, IAcademicWorkbookWriter workbook) : Controller
{
    public async Task<IActionResult> Index(int? termId,int? batchId) => View(await facade.ListAsync(termId,batchId,HttpContext.RequestAborted));
    public async Task<IActionResult> Details(int id) => View(await facade.DetailAsync(id,ct:HttpContext.RequestAborted));
    [HttpPost] public async Task<IActionResult> Generate(int batchId,int termId) => await Run(()=>facade.GenerateAsync(batchId,termId),nameof(Index),new {batchId,termId});
    [HttpPost] public async Task<IActionResult> Status(int id,string status) => await Run(()=>facade.SetOfferingStatusAsync(id,status),nameof(Details),new {id});
    [HttpPost] public async Task<IActionResult> Designate(int id,int facultyId) => await Run(()=>facade.DesignateRetakeMarkerAsync(id,facultyId),nameof(Details),new {id});
    [HttpPost] public async Task<IActionResult> Assign(int id,int facultyId,bool end) => await Run(()=>facade.AssignAsync(id,facultyId,end),nameof(Details),new {id});
    [HttpPost] public async Task<IActionResult> Routine(int id,int routineId,int roomId,byte day,TimeSpan start,TimeSpan end) => await Run(()=>facade.SaveRoutineAsync(id,routineId,roomId,day,start,end),nameof(Details),new {id});
    [HttpPost] public async Task<IActionResult> Register(int batchId,int termId)
    { if(!ModelState.IsValid)return BadRequest();try {var id=await facade.QueueRegistrationAsync(batchId,termId);return RedirectToAction(nameof(Job),new {id});}catch(BusinessRuleException ex){TempData["Error"]=ex.Message;return RedirectToAction(nameof(Index),new {batchId,termId});} }
    public async Task<IActionResult> Job(int id) => View(await facade.JobAsync(id));
    [HttpGet] public async Task<IActionResult> RegistrationError(int id)
    {
        // JobAsync enforces department-scoped registration authorization.
        var job=await facade.JobAsync(id,HttpContext.RequestAborted);
        if(job.Status!="Failed")return NotFound();
        var row=new[]{job.Id.ToString(),job.BatchId.ToString(),job.TermId.ToString(),job.Status,
            job.FinishedAt?.ToString("yyyy-MM-dd HH:mm:ss",System.Globalization.CultureInfo.InvariantCulture)??"",job.FailureReason??""};
        return File(writer.WriteCsv(new[]{"Job ID","Batch ID","Term ID","Status","Finished (Dhaka)","Failure summary"},new[]{row}),"text/csv; charset=utf-8",$"registration-error-{id}.csv");
    }
    [HttpPost] public async Task<IActionResult> Confirm(int batchId,int termId) => await Run(()=>facade.ConfirmAsync(batchId,termId),nameof(Index),new {batchId,termId});
    public async Task<IActionResult> Sections(int batchId) {ViewBag.BatchId=batchId;return View(await facade.SectionReportAsync(batchId));}
    public async Task<IActionResult> RegistrationJobs(int batchId) {ViewBag.BatchId=batchId;return View(await facade.RegistrationJobsAsync(batchId,HttpContext.RequestAborted));}
    public async Task<IActionResult> ExportSections(int batchId,string format="csv")
    {
        if(format is not ("csv" or "xlsx" or "print"))return BadRequest();
        var rows=await facade.SectionReportAsync(batchId);await facade.AuthorizeExportAsync(batchId,format:format);
        return Report("sections",new[]{"Student ID","Name","Section","Status","Published earned credits"},rows.Select(r=>new[]{r.StudentId,r.Name,r.Section,r.Status,r.EarnedCredits.ToString(System.Globalization.CultureInfo.InvariantCulture)}),format);
    }
    public async Task<IActionResult> ExportOffering(int id,string format="csv")
    {
        if(format is not ("csv" or "xlsx" or "print"))return BadRequest();
        var d=await facade.DetailAsync(id);await facade.AuthorizeExportAsync(d.Offering.BatchId,id,format:format);
        return Report("enrollment",new[]{"Student ID","Name","Type","Status","Prerequisite warning","Present","Classes","Attendance percent","Shortage","Attendance marks"},d.Students.Select(r=>new[]{r.StudentId,r.Name,r.Type,r.Status,r.PrerequisiteWarning.ToString(),r.Present.ToString(),r.Classes.ToString(),r.Percent?.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)??"",r.Shortage.ToString(),r.Marks?.ToString(System.Globalization.CultureInfo.InvariantCulture)??""}),format);
    }
    private IActionResult Report(string name,string[] headers,IEnumerable<string[]> rows,string format)
    {
        if(format=="xlsx")return File(workbook.Write(headers,rows),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",name+".xlsx");
        if(format=="print")return View("PrintReport",new UA.Facade.DTO.Academic.AcademicPrintReport(name,headers,rows.ToArray()));
        return File(writer.WriteCsv(headers,rows),"text/csv; charset=utf-8",name+".csv");
    }
    private async Task<IActionResult> Run(Func<Task> action,string target,object route)
    {if(!ModelState.IsValid){TempData["Error"]="Invalid form values.";return RedirectToAction(target,route);}try{await action();TempData["Message"]="Operation saved.";}catch(BusinessRuleException ex){TempData["Error"]=ex.Message;}return RedirectToAction(target,route);}
}
