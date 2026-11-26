using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UA.Facade.DTO.Admission;
using UA.Facade.Interface.Admission;
using UA.Framework.Exceptions;
namespace UA.Web.UI.Areas.Admission.Controllers;
[Area("Admission")]
public sealed class ApplicantController(IAdmissionFacade admission,UA.Framework.Time.IClock clock) : Controller
{
 [AllowAnonymous,HttpGet] public IActionResult Register()=>View();
 [AllowAnonymous,HttpPost,EnableRateLimiting("auth")] public async Task<IActionResult> Register(string email,string password,string mobile){if(!ModelState.IsValid)throw new BusinessRuleException("Complete the registration form.");await admission.RegisterAsync(email,password,mobile);TempData["Message"]="Account created. Sign in and verify your email to apply.";return RedirectToAction("Login","Account",new{area="",returnUrl=Url.Action("Index","Applicant",new{area="Admission"})});}
 [HttpGet] public async Task<IActionResult> Index(){ViewBag.Circulars=await admission.CircularsAsync();return View(await admission.ApplicationsAsync());}
 [HttpGet] public async Task<IActionResult> Apply(int id){var c=(await admission.CircularsAsync()).SingleOrDefault(x=>x.Id==id)??throw new NotFoundException();if(c.Status!="Published"||clock.Today<c.Open||clock.Today>c.Close)throw new BusinessRuleException("Application window is closed or has not opened.");return View(c);}
 [HttpPost,RequestSizeLimit(6_500_000)] public async Task<IActionResult> Apply(int id,int departmentId,string name,DateTime birth,string gender,string address,string guardian,string guardianPhone,string sscBoard,string sscRoll,decimal sscGpa,string hscBoard,string hscRoll,decimal hscGpa,bool consent,IFormFile photo,IFormFile? certificate){if(!ModelState.IsValid||photo==null||photo.Length<1||photo.Length>1_048_576||certificate?.Length>5_000_000)throw new BusinessRuleException("Check all fields and upload a JPEG or PNG photo up to 1 MB; optional PDF up to 5 MB.");using var p=new MemoryStream();await photo.CopyToAsync(p);byte[]? cert=null;if(certificate!=null&&certificate.Length>0){using var s=new MemoryStream();await certificate.CopyToAsync(s);cert=s.ToArray();}var key=await admission.ApplyAsync(id,departmentId,new(name,birth,gender,address,guardian,guardianPhone,sscBoard,sscRoll,sscGpa,hscBoard,hscRoll,hscGpa,consent),p.ToArray(),photo.ContentType,cert);return RedirectToAction(nameof(Invoice),new{id=key});}
 [HttpGet] public async Task<IActionResult> Invoice(int id){Response.Headers["Cache-Control"]="private, no-store";return View(await admission.InvoiceAsync(id));}
 [HttpPost] public async Task<IActionResult> EnrollmentInvoice(int id){await admission.CreateEnrollmentInvoiceAsync(id);return RedirectToAction(nameof(Invoice),new{id});}
 [HttpGet] public async Task<IActionResult> AdmitCard(int id){ViewBag.ApplicationId=id;Response.Headers["Cache-Control"]="private, no-store";return View(await admission.AdmitCardAsync(id));}
 [HttpGet] public async Task<IActionResult> Photo(int id){var p=await admission.PhotoAsync(id);Response.Headers["Cache-Control"]="private, no-store";return File(p.Data,p.Mime);}
}
