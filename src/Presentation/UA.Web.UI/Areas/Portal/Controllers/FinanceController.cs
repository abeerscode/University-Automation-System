using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Finance;
using UA.Facade.Interface.Integration;
namespace UA.Web.UI.Areas.Portal.Controllers;
[Area("Portal")]
public sealed class FinanceController(ILedgerFacade ledger,IPaymentFacade payments,IReceiptPdfWriter pdf) : Controller
{
    [HttpGet] public async Task<IActionResult> Index() => View(await ledger.OwnAsync());
    [HttpGet] public async Task<IActionResult> Invoice(int? termId){Response.Headers["Cache-Control"]="private, no-store";ViewBag.TermId=termId;return View("~/Areas/Finance/Views/Ledger/Invoice.cshtml",await ledger.OwnAsync());}
    [HttpGet] public async Task<IActionResult> Receipt(int id) { var r=await payments.ReceiptAsync(id,true); Response.Headers["Cache-Control"]="private, no-store"; return File(pdf.Write(r),"application/pdf",r.Number+".pdf"); }
}
