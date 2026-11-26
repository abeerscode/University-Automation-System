using UA.Facade.Interface.Student;
namespace UA.API.Workers;
public sealed class LifecycleWarningWorker(IServiceScopeFactory scopes,ILogger<LifecycleWarningWorker> logger):BackgroundService
{
 protected override async Task ExecuteAsync(CancellationToken stoppingToken)
 {
  while(!stoppingToken.IsCancellationRequested)
  {
   try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<ILifecycleWarningScan>().ScanAsync(stoppingToken);}
   catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested){break;}
   catch(Exception ex){logger.LogError(ex,"Lifecycle warning scan failed");}
   try{await Task.Delay(TimeSpan.FromHours(6),stoppingToken);}catch(OperationCanceledException){break;}
  }
 }
}
