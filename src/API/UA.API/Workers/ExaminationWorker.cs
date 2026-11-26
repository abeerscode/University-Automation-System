using UA.Facade.Interface.Examination;
using UA.Framework.Exceptions;
namespace UA.API.Workers;
public sealed class ExaminationWorker(IServiceScopeFactory scopes,ILogger<ExaminationWorker> logger):BackgroundService
{
    private static string SafeReason(Exception ex)=>ex switch
    {
        BusinessRuleException e when e.Message is "Configure exam halls first." or "Insufficient exam hall capacity." or "Insufficient exam hall capacity for concurrent sittings." or "Room already allocated to a concurrent sitting." or "Exam slot clashes with a batch, section or regular examinee." or "Existing exam seats are invalid or duplicated. Correct the seat plan first." or "Existing make-up allocations require the Phase 8 scheduling workflow." or "Every offering with confirmed enrollments must have approved marks." or "Incomplete approved marks." or "No approved enrollments to publish." or "Generate routine first." or "Configure grade scale." or "Configure attendance scale." or "Final examination must finish before publication." => e.Message,
        ForbiddenException => "The requesting user is no longer authorized for this job.",
        _=>"Examination job failed. See API logs for the job ID."
    };
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            int? id=null;
            try{using var scope=scopes.CreateScope();var service=scope.ServiceProvider.GetRequiredService<IExamJobProcessor>();id=await service.NextAsync(stoppingToken);if(id.HasValue)await service.ProcessAsync(id.Value,stoppingToken);}
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested){break;}
            catch(Exception ex){logger.LogError(ex,"Examination job {JobId} failed",id);if(id.HasValue)try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<IExamJobProcessor>().FailAsync(id.Value,SafeReason(ex),stoppingToken);}catch(Exception failure){logger.LogError(failure,"Examination job status update failed");}}
            try{await Task.Delay(TimeSpan.FromSeconds(5),stoppingToken);}catch(OperationCanceledException){break;}
        }
    }
}
