using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.OutputCaching;
using UA.Facade.Interface.Admission;
using UA.Facade.Interface.Website;
namespace UA.Web.UI.Controllers;
[AllowAnonymous]
public sealed class PublicController(IPublicWebsiteFacade website,IAdmissionFacade admission):Controller
{
 [HttpGet,OutputCache(Duration=60,Tags=new[]{"public-site"})] public async Task<IActionResult> Index()=>View(await website.HomeAsync());
 [HttpGet] public async Task<IActionResult> Admission()=>View(await admission.CircularsAsync());
 [HttpGet,OutputCache(Duration=60,Tags=new[]{"public-site"})] public async Task<IActionResult> Faculty()=>View(await website.FacultyAsync());
 [HttpGet,OutputCache(Duration=60,Tags=new[]{"public-site"})] public async Task<IActionResult> Page(string id){if(id=="site-profile")return NotFound();var p=await website.PageAsync(id);return p==null?(id is "about" or "contact"?View("PendingInformation",id):NotFound()):View(p);}
 [HttpGet,OutputCache(Duration=60,Tags=new[]{"public-site"})] public async Task<IActionResult> Department(int id){var home=await website.HomeAsync();var d=home.Departments.FirstOrDefault(x=>x.Id==id);if(d==null)return NotFound();ViewBag.Pages=home.Pages.Where(x=>x.DepartmentId==id).ToArray();return View(d);}
 [HttpGet] public async Task<IActionResult> Notices()=>View((await website.HomeAsync()).Notices);
 [HttpGet] public IActionResult Result(){Response.Headers["Cache-Control"]="private, no-store";return View();}
 [HttpPost,EnableRateLimiting("public-result")] public async Task<IActionResult> Result(int applicantId,int year,DateTime birth){Response.Headers["Cache-Control"]="private, no-store";ViewBag.Searched=true;return View(ModelState.IsValid?await admission.PublicResultAsync(applicantId,year,birth):null);}
}
