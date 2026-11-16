using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UA.DataAccess.Data;

/// <summary>
/// Design-time factory used by dotnet-ef for migrations and database updates.
/// Set UNIVERSITY_DB to the complete SQL Server connection string before
/// running EF commands. No database password is hardcoded in this project.
/// </summary>
public sealed class UniversityDbContextFactory
    : IDesignTimeDbContextFactory<UniversityDbContext>
{
    public UniversityDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("UNIVERSITY_DB");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The UNIVERSITY_DB environment variable is not set. " +
                "Set it to your SQL Server connection string before running dotnet-ef.");
        }

        var options = new DbContextOptionsBuilder<UniversityDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new UniversityDbContext(options);
    }
}
