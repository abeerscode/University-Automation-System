using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
namespace UA.Web.UI.Controllers;
public sealed class FilesController(IUploadedDocumentFacade facade) : Controller
{
    public async Task<IActionResult> Download(int id)
    {
        var x = await facade.ReadAsync(id);
        return File(x.Data, x.ContentType, x.FileName);
    }
}
