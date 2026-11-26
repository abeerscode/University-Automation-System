using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Student;

/// <summary>Checks the existing paper approval record, department, authority and prior consumption.</summary>
public sealed class StudentApprovalGuard(IApprovalRecordRepository approvals,
    IRepository<Department> departments, IRepository<UserRole> assignments, IRepository<Role> roles,
    IRepository<StudentStatusHistory> statusHistory, IRepository<StudentBatchHistory> placements,
    IRepository<AuditEntry> audits, IClock clock)
{
    public async Task<ApprovalRecord> RequireAsync(string reference, string approvalType,
        int departmentId, int? requestorUserId, bool departmentHeadRequired, CancellationToken ct)
    {
        reference = reference?.Trim() ?? "";
        var approval = await approvals.FirstAsync(x => x.ReferenceNo == reference, ct)
            ?? throw new BusinessRuleException("Record the physical approval in Administration > Approvals before proceeding.");
        var department = await departments.FirstAsync(x => x.DepartmentId == departmentId, ct)
            ?? throw new NotFoundException("Department not found.");
        if (!string.Equals(approval.ApprovalType, approvalType, StringComparison.OrdinalIgnoreCase) ||
            approval.Status != "Approved" || approval.ApprovalDate.Date > clock.Today ||
            !approval.ReferenceNo.StartsWith(department.Code + "-", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"An approved {approvalType} reference for this department is required.");
        // A-13 is enforced when the approval is recorded. For a separate staff
        // request (e.g. a coordinator's section move), verify the requester again.
        if (requestorUserId.HasValue && approval.ApproverId == requestorUserId.Value)
            throw new BusinessRuleException("A person cannot approve their own request.");
        if (await audits.FirstAsync(x => x.Action == "ApprovedStudentEdit" &&
            x.NewValue == "Approval reference: " + reference, ct) is not null)
            throw new BusinessRuleException("This approval reference was already used for a student edit.");
        if (await statusHistory.FirstAsync(x => x.ApprovalId == approval.ApprovalId, ct) is not null ||
            await placements.FirstAsync(x => x.ApprovalId == approval.ApprovalId, ct) is not null)
            throw new BusinessRuleException("This approval reference has already been used for a student change.");
        if (departmentHeadRequired)
        {
            var headRole = await roles.FirstAsync(x => x.Name == RoleNames.Head, ct)
                ?? throw new BusinessRuleException("Department Head role is missing.");
            if (await assignments.FirstAsync(x => x.UserId == approval.ApproverId &&
                x.RoleId == headRole.RoleId && x.DepartmentId == departmentId, ct) is null)
                throw new BusinessRuleException("The recorded approver must be the Department Head of this department.");
        }
        return approval;
    }
}
