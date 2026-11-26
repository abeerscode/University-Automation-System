using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
namespace UA.API.Workers;
public sealed class RegistrationWorker(IServiceScopeFactory scopes, ILogger<RegistrationWorker> logger) : BackgroundService
{
    // Do not expose database exception messages, SQL, or connection details to staff.
    private static string SafeReason(Exception ex) => ex switch
    {
        BusinessRuleException e when e.Message == "Generate complete section offerings before registration." => e.Message,
        BusinessRuleException e when e.Message == "Curriculum is empty." => e.Message,
        BusinessRuleException e when e.Message == "Job requested outside registration window." => e.Message,
        BusinessRuleException e when e.Message == "No curriculum credits for the attended semester." => e.Message,
        ForbiddenException => "The responsible account no longer has registration permission.",
        NotFoundException => "A required academic record was not found.",
        BusinessRuleException => "An academic or fee validation rule prevented registration. Ask an administrator to check the API logs.",
        _ => "Registration could not complete. Ask an administrator to check the API logs for this job ID."
    };
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextScan=DateTime.MinValue;
        while (!stoppingToken.IsCancellationRequested)
        {
            int? id=null;
            if(DateTime.UtcNow>=nextScan)
            {
                nextScan=DateTime.UtcNow.AddMinutes(1);
                try {using var scan=scopes.CreateScope();await scan.ServiceProvider.GetRequiredService<IRegistrationJobProcessor>().ScheduleOpenWindowsAsync(stoppingToken);}
                catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested){break;}
                catch(Exception ex){logger.LogError(ex,"Automatic registration scan failed");}
            }
            try {
                using var scope=scopes.CreateScope();var processor=scope.ServiceProvider.GetRequiredService<IRegistrationJobProcessor>();
                id=await processor.NextAsync(stoppingToken);if(id.HasValue)await processor.ProcessAsync(id.Value,stoppingToken);
            } catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { break; }
            catch(Exception ex) {
                logger.LogError(ex,"Registration job {JobId} failed",id);
                if(id.HasValue)try {using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<IRegistrationJobProcessor>().FailAsync(id.Value,SafeReason(ex),stoppingToken);}catch(Exception failure){logger.LogError(failure,"Could not update registration job status");}
            }
            try {await Task.Delay(TimeSpan.FromSeconds(5),stoppingToken);}catch(OperationCanceledException){break;}
        }
    }
}
