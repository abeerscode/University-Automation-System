using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Student;
using UA.Facade.Interface.Integration;
using UA.Framework.Exceptions;
namespace UA.Web.UI.Areas.Student.Controllers;
[Area("Student"),Authorize]
public sealed class LifecycleController(ILifecycleFacade facade,IExcelWriter csv):Controller
{
 public async Task<IActionResult> Index()=>View(await facade.ListAsync(HttpContext.RequestAborted));
 public async Task<IActionResult> Details(string? id)=>View(await facade.DetailsAsync(id,HttpContext.RequestAborted));
 public async Task<IActionResult> Shortlist()=>View(await facade.ShortlistAsync(HttpContext.RequestAborted));
 public async Task<IActionResult> GraduationList(string format="print")
 {var rows=(await facade.ListAsync(HttpContext.RequestAborted)).Where(x=>x.Status is "Graduated" or "Alumni").Select(x=>new[]{x.StudentId,x.Name,x.DepartmentId.ToString(),x.Status}).ToArray();await facade.AuditGraduationExportAsync(HttpContext.RequestAborted);string[] headers=["Student ID","Name","Department","Status"];return format=="csv"?File(csv.WriteCsv(headers,rows),"text/csv; charset=utf-8","graduation-list.csv"):View("~/Areas/Academic/Views/Operations/PrintReport.cshtml",new UA.Facade.DTO.Academic.AcademicPrintReport("Graduation list",headers,rows));}
 [HttpPost] public Task<IActionResult> RequestDrop(string id,int termId)=>Run(()=>facade.RequestDropAsync(id,termId),id);
 [HttpPost] public Task<IActionResult> RecommendDrop(string id)=>Run(()=>facade.RecommendDropAsync(id),id);
 [HttpPost] public Task<IActionResult> DecideDrop(string id,int termId,string reference)=>Run(()=>facade.DecideDropAsync(id,termId,reference),id);
 [HttpPost] public Task<IActionResult> Rejoin(string id,int termId)=>Run(()=>facade.RejoinAsync(id,termId),id);
 [HttpPost] public Task<IActionResult> Equivalence(string id,int oldCourseId,int newCourseId,string reference)=>Run(()=>facade.EquivalenceAsync(id,oldCourseId,newCourseId,reference),id);
 [HttpPost] public Task<IActionResult> Graduate(string id,bool alumni)=>Run(()=>facade.GraduateAsync(id,alumni),id);
 [HttpPost] public Task<IActionResult> DecideLimit(string id,string status,string reference)=>Run(()=>facade.DecideLimitAsync(id,status,reference),id);
 [HttpPost] public Task<IActionResult> Retake(string id,int offeringId,string? reference,string? reason)=>Run(()=>facade.RetakeAsync(id,offeringId,reference,reason),id);
 [HttpPost] public Task<IActionResult> SpecialRetake(string id,int courseId,int termId)=>Run(()=>facade.SpecialRetakeAsync(id,courseId,termId),id);
 public async Task<IActionResult> Schemes()=>View(await facade.SchemesAsync(HttpContext.RequestAborted));
 [HttpPost] public async Task<IActionResult> SaveScheme(int departmentId,int termId,string type,UA.Facade.DTO.Student.RetakeComponentInput[] parts){if(!ModelState.IsValid){TempData["Error"]="Invalid components.";return RedirectToAction(nameof(Schemes));}try{await facade.SaveSchemeAsync(departmentId,termId,type,parts,HttpContext.RequestAborted);TempData["Message"]="Scheme recorded.";}catch(BusinessRuleException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Schemes));}
 [HttpPost] public Task<IActionResult> RequestDocument(string type)=>Run(()=>facade.RequestDocumentAsync(type),null);
 [HttpPost] public Task<IActionResult> ProcessDocument(string id,int requestId,bool issue)=>Run(()=>facade.ProcessDocumentAsync(requestId,issue),id);
 private async Task<IActionResult> Run(Func<Task> action,string? id){if(!ModelState.IsValid){TempData["Error"]="Invalid form values.";return RedirectToAction(nameof(Details),new{id});}try{await action();TempData["Message"]="Operation recorded.";}catch(BusinessRuleException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Details),new{id});}
}
