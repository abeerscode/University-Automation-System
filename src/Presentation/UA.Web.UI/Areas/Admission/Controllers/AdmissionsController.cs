using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Admission;
using UA.Facade.Interface.Admission;
using UA.Facade.Interface.Integration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using System.Globalization;
namespace UA.Web.UI.Areas.Admission.Controllers;
[Area("Admission")]
public sealed class AdmissionsController(IAdmissionFacade admission,IStudentImportReader reader,IAcademicWorkbookWriter workbook,IAdmissionReportJobs reportJobs) : Controller
{
 [HttpGet] public async Task<IActionResult> Index(){ViewBag.Departments=await admission.DepartmentsAsync();return View(await admission.CircularsAsync(true));}
 [HttpPost] public async Task<IActionResult> Create(short year,DateTime open,DateTime close,DateTime test,string venue,DateTime result,DateTime deadline,int[] departmentIds,short[] seats){if(!ModelState.IsValid)throw new BusinessRuleException("Invalid circular input.");await admission.CreateCircularAsync(new(year,open,close,test,venue,result,deadline,departmentIds,seats));return RedirectToAction(nameof(Index));}
 [HttpPost] public async Task<IActionResult> Publish(int id){await admission.PublishCircularAsync(id);return RedirectToAction(nameof(Index));}
 [HttpPost] public async Task<IActionResult> Edit(int id,DateTime close,DateTime test,string venue,DateTime result,DateTime deadline,string reason){if(!ModelState.IsValid)throw new BusinessRuleException("Invalid circular input.");await admission.UpdateCircularAsync(id,close,test,venue,result,deadline,reason);return RedirectToAction(nameof(Index));}
 [HttpPost] public async Task<IActionResult> Close(int id,string reason){await admission.CloseCircularAsync(id,reason);return RedirectToAction(nameof(Index));}
 private static IReadOnlyList<AdmissionRow> Filter(IReadOnlyList<AdmissionRow> rows,int? departmentId,string? status)=>rows.Where(x=>(!departmentId.HasValue||x.DepartmentId==departmentId)&& (string.IsNullOrWhiteSpace(status)||x.Status==status)).ToArray();
 [HttpGet] public async Task<IActionResult> Applications(int id,int? departmentId,string? status){ViewBag.CircularId=id;ViewBag.Departments=await admission.DepartmentsAsync();ViewBag.DepartmentId=departmentId;ViewBag.Status=status;return View(Filter(await admission.ApplicationsAsync(id,true),departmentId,status));}
 [HttpGet] public async Task<IActionResult> Invoice(int id){Response.Headers["Cache-Control"]="private, no-store";return View("~/Areas/Admission/Views/Applicant/Invoice.cshtml",await admission.InvoiceAsync(id,true));}
 [HttpGet] public async Task<IActionResult> Documents(int id){ViewBag.ApplicationId=id;return View(await admission.DocumentsAsync(id,true));}
 [HttpGet] public async Task<IActionResult> Download(int id,int documentId){var d=await admission.DocumentAsync(id,documentId,true);Response.Headers["Cache-Control"]="private, no-store";return File(d.Data,d.Mime,"document-"+documentId+(d.Mime=="application/pdf"?".pdf":d.Mime=="image/png"?".png":".jpg"));}
 [HttpPost] public async Task<IActionResult> Verify(int id){await admission.VerifyDocumentsAsync(id);return RedirectToAction(nameof(Documents),new{id});}
 [HttpPost] public async Task<IActionResult> Confirm(int id){Response.Headers["Cache-Control"]="private, no-store";return View("Credential",await admission.ConfirmAsync(id));}
 [HttpPost] public async Task<IActionResult> Replace(int id,string reference,int circularId){await admission.ReplaceAsync(id,reference);return RedirectToAction(nameof(Applications),new{id=circularId});}
 [HttpGet] public async Task<IActionResult> Template(int id){await admission.ApplicationsAsync(id,true);return File(workbook.Write(["ApplicantId","Marks","Rank"],Array.Empty<string[]>()),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","admission-test-template.xlsx");}
 [HttpPost,RequestSizeLimit(2_200_000)] public async Task<IActionResult> Import(int id,IFormFile upload){if(upload==null||upload.Length<1||upload.Length>2_000_000)throw new BusinessRuleException("Upload a CSV or XLSX file up to 2 MB.");await admission.ApplicationsAsync(id,true);using var ms=new MemoryStream();await upload.CopyToAsync(ms);var sheet=reader.Read(ms.ToArray(),Path.GetExtension(upload.FileName));var rows=new List<TestResultInput>();var errors=new List<string>();foreach(var r in sheet.Rows){if(r.Values.ContainsKey("__error")||!r.Values.TryGetValue("ApplicantId",out var a)||!int.TryParse(a,out int applicant)||!r.Values.TryGetValue("Marks",out var m)||!decimal.TryParse(m,NumberStyles.Number,CultureInfo.InvariantCulture,out var marks)||!r.Values.TryGetValue("Rank",out var k)||!int.TryParse(k,out int rank)){errors.Add($"Row {r.Line}: ApplicantId, Marks and Rank required.");continue;}rows.Add(new(r.Line,applicant,marks,rank));}if(errors.Count==0)errors.AddRange(await admission.ImportResultsAsync(id,rows));if(errors.Count>0){ViewBag.CircularId=id;return View("ImportErrors",errors);}TempData["Message"]="Test results imported; not published yet.";return RedirectToAction(nameof(Applications),new{id});}
 [HttpPost] public async Task<IActionResult> PublishMerit(int id){await admission.PublishMeritAsync(id);return RedirectToAction(nameof(Applications),new{id});}
 [HttpPost] public async Task<IActionResult> ExportPdf(int id,int? departmentId,string? status)=>RedirectToAction(nameof(Report),new{id=await reportJobs.EnqueueAsync(id,departmentId,status,true)});
 [HttpPost] public async Task<IActionResult> Export(int id,int? departmentId,string? status)=>RedirectToAction(nameof(Report),new{id=await reportJobs.EnqueueAsync(id,departmentId,status,false)});
 [HttpGet] public async Task<IActionResult> Report(int id){Response.Headers["Cache-Control"]="private, no-store";return View(await reportJobs.GetAsync(id));}
 [HttpGet] public async Task<IActionResult> ReportStatus(int id){Response.Headers["Cache-Control"]="private, no-store";var j=await reportJobs.GetAsync(id);return Json(new{status=j.Status,progress=j.Progress});}
 [HttpGet] public async Task<IActionResult> DownloadReport(int id){Response.Headers["Cache-Control"]="private, no-store";var r=await reportJobs.DownloadAsync(id);return File(r.Data,r.Pdf?"application/pdf":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",r.Pdf?"admission-report.pdf":"admission-report.xlsx");}
}
