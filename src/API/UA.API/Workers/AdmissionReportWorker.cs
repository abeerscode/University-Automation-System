using UA.Facade.Interface.Admission;
namespace UA.API.Workers;
public sealed class AdmissionReportWorker(IServiceScopeFactory scopes,ILogger<AdmissionReportWorker> logger):BackgroundService
{
 protected override async Task ExecuteAsync(CancellationToken ct){var nextCleanup=DateTime.MinValue;while(!ct.IsCancellationRequested){int? id=null;try{using var scope=scopes.CreateScope();var jobs=scope.ServiceProvider.GetRequiredService<IAdmissionReportJobs>();if(DateTime.UtcNow>=nextCleanup){nextCleanup=DateTime.UtcNow.AddHours(1);await jobs.CleanupAsync(ct);}id=await jobs.NextAsync(ct);if(id.HasValue)await jobs.ProcessAsync(id.Value,ct);}catch(OperationCanceledException) when(ct.IsCancellationRequested){break;}catch(Exception ex){logger.LogError(ex,"Admission report job {JobId} failed",id);if(id.HasValue)try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<IAdmissionReportJobs>().FailAsync(id.Value,ct);}catch(Exception failure){logger.LogError(failure,"Report failure state could not be saved");}}try{await Task.Delay(TimeSpan.FromSeconds(5),ct);}catch(OperationCanceledException){break;}}}
}
