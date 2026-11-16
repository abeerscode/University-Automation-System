using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class BackgroundJobFacade(IBackgroundJobRepository jobs, IUnitOfWork work,
    ICurrentUser current, IClock clock, IUserAccessFacade access) : IBackgroundJobFacade
{
    private static JobDto Map(BackgroundJob x) => new(x.JobId, x.JobType, x.RequestedBy, x.Status, x.ProgressPercent, x.CreatedAt, x.ErrorReportPath);
    public async Task<IReadOnlyList<JobDto>> ListAsync(CancellationToken ct = default)
    {
        if (current.UserId is not int id) throw new ForbiddenException();
        return (await jobs.ListAsync(x => x.RequestedBy == id, ct)).OrderByDescending(x => x.JobId).Take(100).Select(Map).ToList();
    }
    public async Task<JobDto?> GetAsync(int id, CancellationToken ct = default)
    {
        if (current.UserId is not int actor) throw new ForbiddenException();
        var job = await jobs.FirstAsync(x => x.JobId == id && x.RequestedBy == actor, ct);
        return job is null ? null : Map(job);
    }
    public async Task<int> EnqueueAsync(string jobType, CancellationToken ct = default)
    {
        await access.RequireAsync(jobType == "Import" ? "imports.run" : "backup.manage", ct: ct);
        if (current.UserId is not int id) throw new ForbiddenException();
        if (jobType is not ("Import" or "Backup")) throw new BusinessRuleException("Unsupported job type.");
        var job = new BackgroundJob { JobType = jobType, RequestedBy = id, Status = "Queued", CreatedAt = clock.Now };
        await jobs.AddAsync(job, ct); await work.SaveChangesAsync(ct); return job.JobId;
    }
}
