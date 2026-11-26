using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Website;
using UA.Facade.Interface.Admission;
using UA.Framework.Exceptions;
namespace UA.Web.UI.Areas.Website.Controllers;
[Area("Website")]
public sealed class PagesController(IPublicWebsiteFacade pages,IAdmissionFacade admission,Microsoft.AspNetCore.OutputCaching.IOutputCacheStore cache,Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,UA.Facade.Interface.Administration.IUserAccessFacade access):Controller
{
 [HttpGet] public async Task<IActionResult> Index(){ViewBag.Departments=await admission.DepartmentsAsync();return View(await pages.HistoryAsync());}
 [HttpPost] public async Task<IActionResult> Draft(string slug,string title,int? departmentId,string content){if(!ModelState.IsValid)throw new BusinessRuleException("Invalid page input.");await pages.DraftAsync(slug,title,departmentId,content);return RedirectToAction(nameof(Index));}
 [HttpPost] public async Task<IActionResult> Profile(string name,string? logo,string? address,string? email,string? telephone,string? facebook,string? linkedIn){var profile=UA.Facade.Rules.Website.PublicContent.Validate(new(name??"",logo??"",address??"",email??"",telephone??"",facebook??"",linkedIn??""));await pages.DraftAsync("site-profile","University profile",null,System.Text.Json.JsonSerializer.Serialize(profile));return RedirectToAction(nameof(Index));}
 [HttpPost,RequestSizeLimit(1_200_000)] public async Task<IActionResult> UploadImage(IFormFile image){await access.RequireGlobalAsync("pages.edit");if(image==null||image.Length<8||image.Length>1_048_576)throw new BusinessRuleException("Choose a public PNG/JPEG image up to 1 MB.");using var ms=new MemoryStream();await image.CopyToAsync(ms);var data=ms.ToArray();var ext=data.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})?".png":data[0]==255&&data[1]==216&&data[2]==255?".jpg":throw new BusinessRuleException("Invalid PNG/JPEG image.");var folder=Path.Combine(env.WebRootPath,"cms-assets");Directory.CreateDirectory(folder);var file=Guid.NewGuid().ToString("N")+ext;await System.IO.File.WriteAllBytesAsync(Path.Combine(folder,file),data);TempData["Message"]="Public image path: /cms-assets/"+file;return RedirectToAction(nameof(Index));}
 [HttpPost] public async Task<IActionResult> Approve(int id){await pages.ApproveAsync(id);await cache.EvictByTagAsync("public-site",HttpContext.RequestAborted);return RedirectToAction(nameof(Index));}
}
