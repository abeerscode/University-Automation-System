using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UA.Framework.Exceptions;
namespace UA.Web.UI.Infrastructure.Filters;
public sealed class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;
        int status = exception switch
        {
            ForbiddenException => 403, NotFoundException => 404,
            BusinessRuleException => 400, InvalidOperationException => 409,
            _ => 500
        };
        if (status >= 500) logger.LogError(exception, "Unhandled request failure");
        context.HttpContext.Response.StatusCode = status;
        var viewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
            new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState);
        viewData["StatusCode"] = status;
        viewData["Message"] = status == 500 ? "An unexpected error occurred." : exception.Message;
        context.Result = new ViewResult { ViewName = "~/Views/Shared/RequestFailure.cshtml", ViewData = viewData };
        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}
