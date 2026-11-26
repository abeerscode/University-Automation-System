using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Student;
using UA.Entities;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Student;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Student;
public sealed class SectionChangeFacade(IStudentRepository students, IRepository<StudentBatchHistory> history,
    IRepository<Section> sections, IRepository<UA.Entities.Student> population, IRepository<AcademicTerm> terms,
    IRepository<UserRole> assignments,
    IRepository<Role> roles, IConfigurationFacade config, IUserAccessFacade access,
    ICurrentUser current, IClock clock, IUnitOfWork work, StudentApprovalGuard approvals) : ISectionChangeFacade
{
    public async Task ChangeAsync(SectionChangeInput input, CancellationToken ct = default)
    {
        await work.ExecuteTransactionAsync(async token =>
        {
            var student = await students.GetTrackedAsync(input.StudentId, token) ?? throw new NotFoundException();
            await access.RequireAsync("section.change", student.DepartmentId, token);
            // Section changes are Coordinator-only even for users with broad grants.
            var coordinator = await roles.FirstAsync(x => x.Name == RoleNames.Coordinator, token)
                ?? throw new ForbiddenException("Department Coordinator role is missing.");
            if (current.UserId is not int actor || await assignments.FirstAsync(x => x.UserId == actor &&
                x.RoleId == coordinator.RoleId && x.DepartmentId == student.DepartmentId, token) is null)
                throw new ForbiddenException("Only this department's Coordinator can change sections.");
            if (student.Status != "Active") throw new BusinessRuleException("Only Active students can change sections.");
            if (student.SectionId == input.SectionId) throw new BusinessRuleException("The student is already in that section.");
            var section = await sections.FirstAsync(x => x.SectionId == input.SectionId &&
                x.BatchId == student.CurrentBatchId, token)
                ?? throw new BusinessRuleException("Choose a section within the student's current batch.");
            if (string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Trim().Length > 255)
                throw new BusinessRuleException("An explanation of the move is required.");
            // Existing configuration takes precedence; A-03 default overflow is five.
            var term = (await terms.ListAsync(ct: token)).Where(x => x.StartDate.Date <= clock.Today)
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.TermNo).FirstOrDefault();
            string? value = term is null ? null : await config.ResolveAsync(ConfigKeys.SectionOverflow,
                student.DepartmentId, term.TermId, token);
            int overflow = int.TryParse(value, out int configured) ? configured : 5;
            if (overflow is < 0 or > 100) throw new BusinessRuleException("Configured section overflow is invalid.");
            int occupied = await population.CountAsync(x => x.SectionId == section.SectionId && x.Status == "Active", token);
            if (occupied >= section.Capacity + overflow)
                throw new BusinessRuleException("Section capacity including allowed overflow is full; open another section.");
            var approval = await approvals.RequireAsync(input.ApprovalReference, "SectionChange",
                student.DepartmentId, actor, true, token);
            var open = student.StudentBatchHistories.SingleOrDefault(x => x.ToDate == null);
            if (open is null) throw new BusinessRuleException("Student's current placement history is missing.");
            if (open.FromDate.Date > clock.Today)
                throw new BusinessRuleException("Current placement has a future start date.");
            open.ToDate = clock.Today;
            await history.AddAsync(new StudentBatchHistory { StudentId = student.StudentId,
                BatchId = student.CurrentBatchId, SectionId = section.SectionId, FromDate = clock.Today,
                Reason = input.Reason.Trim(), ApprovalId = approval.ApprovalId }, token);
            student.SectionId = section.SectionId;
        }, ct);
    }
}
