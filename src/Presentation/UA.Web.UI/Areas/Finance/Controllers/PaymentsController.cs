using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Finance;
using UA.Facade.Interface.Integration;
namespace UA.Web.UI.Areas.Finance.Controllers;
[Area("Finance")]
public sealed class PaymentsController(IPaymentFacade payments,IReceiptPdfWriter pdf) : Controller
{
    [HttpGet] public async Task<IActionResult> Receipt(int id) { var r=await payments.ReceiptAsync(id,false); Response.Headers["Cache-Control"]="private, no-store"; return File(pdf.Write(r),"application/pdf",r.Number+".pdf"); }
}
