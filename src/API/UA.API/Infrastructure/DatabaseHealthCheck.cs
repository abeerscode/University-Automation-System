using Microsoft.Extensions.Diagnostics.HealthChecks;
using UA.Facade.Interface.Administration;
namespace UA.API.Infrastructure;
public sealed class DatabaseHealthCheck(IPlatformOperationsFacade facade) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => await facade.CheckDatabaseAsync(cancellationToken) ? HealthCheckResult.Healthy("Database available")
            : HealthCheckResult.Unhealthy("Database is unavailable");
}
