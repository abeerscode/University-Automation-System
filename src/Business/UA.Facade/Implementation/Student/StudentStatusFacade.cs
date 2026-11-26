using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Student;
using UA.Entities;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Student;
using UA.Facade.Rules.Student;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Student;
public sealed class StudentStatusFacade(IStudentRepository students, IRepository<StudentStatusHistory> history,
    IUserAccessFacade access, ICurrentUser current, IClock clock, IUnitOfWork work,
    StudentApprovalGuard approvals, IStudentStatusTransitionPolicy policy) : IStudentStatusFacade
{
    public async Task ChangeAsync(StudentStatusChangeInput input, CancellationToken ct = default)
    {
        await work.ExecuteTransactionAsync(async token =>
        {
            var student = await students.GetTrackedAsync(input.StudentId, token) ?? throw new NotFoundException();
            await access.RequireAsync("status.change", student.DepartmentId, token);
            policy.EnsurePermitted(student.Status, input.Status);
            if (string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Trim().Length > 255 ||
                input.EffectiveDate.Date > clock.Today || input.EffectiveDate.Date < new DateTime(student.AdmissionBatch.AdmissionYear,1,1))
                throw new BusinessRuleException("A valid reason and effective date up to today are required.");
            if (student.StudentStatusHistories.Any(x => x.EffectiveDate.Date > input.EffectiveDate.Date))
                throw new BusinessRuleException("Effective date cannot precede an existing status change.");
            var approval = await approvals.RequireAsync(input.ApprovalReference, "StudentStatus", student.DepartmentId,
                null, false, token);
            student.Status = input.Status;
            await history.AddAsync(new StudentStatusHistory { StudentId = student.StudentId,
                Status = input.Status, EffectiveDate = input.EffectiveDate.Date,
                Reason = input.Reason.Trim(), ChangedBy = current.UserId!.Value,
                ApprovalId = approval.ApprovalId }, token);
        }, ct);
    }
}
