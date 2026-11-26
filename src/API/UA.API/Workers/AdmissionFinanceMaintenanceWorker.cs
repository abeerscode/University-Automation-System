using UA.Facade.Interface.Admission;
using UA.Facade.Interface.Finance;
namespace UA.API.Workers;
public sealed class AdmissionFinanceMaintenanceWorker(IServiceScopeFactory scopes,ILogger<AdmissionFinanceMaintenanceWorker> logger):BackgroundService
{
 protected override async Task ExecuteAsync(CancellationToken stoppingToken){while(!stoppingToken.IsCancellationRequested){
  await Run<IAdmissionMaintenance>(s=>s.RemindAsync(stoppingToken));
  if(Enabled("UAS_ENABLE_APPLICANT_ANONYMIZATION"))await Run<IAdmissionMaintenance>(s=>s.AnonymizeAsync(stoppingToken));
  if(Enabled("UAS_ENABLE_LATE_FEES"))await Run<ILateFeeScan>(s=>s.ScanAsync(stoppingToken));
  try{await Task.Delay(TimeSpan.FromHours(6),stoppingToken);}catch(OperationCanceledException){break;}
 }}
 private static bool Enabled(string key)=>string.Equals(Environment.GetEnvironmentVariable(key),"true",StringComparison.OrdinalIgnoreCase);
 private async Task Run<T>(Func<T,Task> action) where T:notnull{try{using var scope=scopes.CreateScope();await action(scope.ServiceProvider.GetRequiredService<T>());}catch(OperationCanceledException){ }catch(Exception ex){logger.LogError(ex,"Maintenance scan {Service} failed",typeof(T).Name);}}
}
