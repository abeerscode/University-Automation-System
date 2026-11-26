using Microsoft.Extensions.Configuration;
using UA.DataAccess.Data;
using UA.DataAccess.Data.Seed;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class UniversityBootstrapper(UniversityDbContext db, IClock clock,
    IPasswordHasher hasher, UA.Facade.Rules.Administration.IPasswordPolicy policy) : IUniversityBootstrapper
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var password = Environment.GetEnvironmentVariable("UAS_BOOTSTRAP_ADMIN_PASSWORD");
        if (!string.IsNullOrEmpty(password) && policy.Validate(password) is string issue)
            throw new InvalidOperationException("Bootstrap administrator password rejected: " + issue);
        await DbInitializer.SeedAsync(db, clock, hasher.Hash,
            Environment.GetEnvironmentVariable("UAS_BOOTSTRAP_ADMIN_USERNAME"),
            password, Environment.GetEnvironmentVariable("UAS_BOOTSTRAP_ADMIN_EMAIL"), ct);
        if (string.Equals(Environment.GetEnvironmentVariable("UAS_SEED_DEMO"), "true", StringComparison.OrdinalIgnoreCase))
            await Phase5DemoSeeder.SeedAsync(db, clock, hasher.Hash, ct);
    }
}
