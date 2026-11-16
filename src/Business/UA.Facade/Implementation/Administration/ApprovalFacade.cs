using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Facade.Rules.Administration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Administration;
public sealed class ApprovalFacade(IApprovalRecordRepository approvals, IRepository<Department> departments,
    IUnitOfWork work, ICurrentUser current, IUserAccessFacade access, ISelfApprovalGuard guard, IClock clock) : IApprovalFacade
{
    private static ApprovalDto Map(ApprovalRecord x) => new(x.ApprovalId, x.ReferenceNo, x.ApprovalType, x.Status,
        x.ApproverId, x.ApprovalDate, x.SupersedesId, x.ScanPath);
    public async Task<IReadOnlyList<ApprovalDto>> ListAsync(CancellationToken ct = default)
    {
        await access.RequireAsync("approvals.record", ct: ct);
        var all = (await approvals.ListAsync(ct: ct)).OrderByDescending(x => x.ApprovalId).Take(500);
        var allowed = new List<ApprovalDto>();
        var codes = await departments.ListAsync(ct: ct);
        foreach (var x in all)
        {
            var part = x.ReferenceNo.Split('-')[0];
            var dept = codes.FirstOrDefault(d => d.Code.Equals(part, StringComparison.OrdinalIgnoreCase));
            if (dept is null) {
                try { await access.RequireGlobalAsync("approvals.record", ct); allowed.Add(Map(x)); }
                catch (ForbiddenException) { }
            } else if (await access.AllowedAsync(current.UserId!.Value, "approvals.record", dept.DepartmentId, ct)) allowed.Add(Map(x));
        }
        return allowed.Take(200).ToList();
    }
    public async Task<ApprovalDto> RecordAsync(string type, string status, int? departmentId, int requestorId,
        string? scanPath, CancellationToken ct = default)
    {
        if (departmentId is null) await access.RequireGlobalAsync("approvals.record", ct);
        else await access.RequireAsync("approvals.record", departmentId, ct);
        if (current.UserId is not int approverId) throw new ForbiddenException();
        guard.Check(approverId, requestorId);
        ApprovalRecord? x = null;
        await work.ExecuteTransactionAsync(async token =>
        {
            x = await NewRecordAsync(type, status, departmentId, approverId, null, scanPath, token);
            await approvals.AddAsync(x, token);
        }, ct);
        return Map(x!);
    }
    public async Task<ApprovalDto> CorrectAsync(int approvalId, string status, string? scanPath, CancellationToken ct = default)
    {
        await access.RequireAsync("approvals.correct", ct: ct);
        var old = await approvals.FirstAsync(x => x.ApprovalId == approvalId, ct) ?? throw new NotFoundException();
        if (current.UserId is not int approverId) throw new ForbiddenException();
        // Corrections never modify the old approval (BR-24).
        var deptCode = old.ReferenceNo.Split('-')[0];
        var dept = await departments.FirstAsync(x => x.Code == deptCode, ct);
        if (dept is null) await access.RequireGlobalAsync("approvals.correct", ct);
        else await access.RequireAsync("approvals.correct", dept.DepartmentId, ct);
        ApprovalRecord? next = null;
        await work.ExecuteTransactionAsync(async token =>
        {
            next = await NewRecordAsync(old.ApprovalType, status, dept?.DepartmentId, approverId, old.ApprovalId, scanPath, token);
            await approvals.AddAsync(next, token);
        }, ct);
        return Map(next!);
    }
    private async Task<ApprovalRecord> NewRecordAsync(string type, string status, int? deptId,
        int approverId, int? supersedes, string? scan, CancellationToken ct)
    {
        if (type.Length is < 1 or > 30 || status.Length is < 1 or > 15) throw new BusinessRuleException("Invalid type or status.");
        var dept = deptId is int id ? await departments.FirstAsync(x => x.DepartmentId == id, ct) : null;
        if (deptId is not null && dept is null) throw new NotFoundException("Department not found.");
        var deptPart = dept?.Code.ToUpperInvariant() ?? "UNI";
        var typePart = new string(type.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (typePart.Length == 0) throw new BusinessRuleException("Invalid approval type.");
        typePart = typePart[..Math.Min(typePart.Length, 8)];
        var prefix = $"{deptPart}-{typePart}-{clock.Now.Year}-";
        var all = await approvals.ListAsync(x => x.ReferenceNo.StartsWith(prefix), ct);
        int next = all.Select(x => int.TryParse(x.ReferenceNo.Split('-').Last(), out int n) ? n : 0).DefaultIfEmpty(0).Max() + 1;
        if (next > 9999) throw new BusinessRuleException("Annual reference sequence exhausted.");
        return new ApprovalRecord { ReferenceNo = prefix + next.ToString("D4"), ApprovalType = type,
            Status = status, ApproverId = approverId, ApprovalDate = clock.Today,
            SupersedesId = supersedes, ScanPath = scan };
    }
}
