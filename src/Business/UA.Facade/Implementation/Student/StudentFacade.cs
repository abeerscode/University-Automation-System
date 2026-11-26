using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Student;
using UA.Entities;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Student;
using UA.Framework.Common;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Student;
public sealed class StudentFacade(IStudentRepository students, IRepository<Department> departments,
    IUserAccessFacade access, ICurrentUser current, IUnitOfWork work,
    StudentApprovalGuard approvals, IRepository<AuditEntry> audit, IClock clock) : IStudentFacade
{
    public async Task<PagedResult<StudentListItem>> SearchAsync(StudentSearchInput input, CancellationToken ct = default)
    {
        await access.RequireAsync("students.view", ct: ct);
        if (current.UserId is not int userId) throw new ForbiddenException();
        int page = Math.Clamp(input.Page, 1, 100000), take = Math.Clamp(input.PageSize, 1, 100);
        var all = await departments.ListAsync(ct: ct);
        var authorized = new List<int>();
        foreach (var d in all)
            if (await access.AllowedAsync(userId, "students.view", d.DepartmentId, ct)) authorized.Add(d.DepartmentId);
        var (rows, total) = await students.SearchAsync(input.Query, authorized, input.DepartmentId,
            input.BatchId, input.SectionId, input.Status, (page - 1) * take, take, ct);
        return new PagedResult<StudentListItem>(rows.Select(x => new StudentListItem(x.StudentId,
            x.FullName, x.Department.Code, x.DepartmentId, x.AdmissionBatch.AdmissionYear,
            x.CurrentBatch.AdmissionYear, x.Section.Name, x.Status, x.RollNo)).ToArray(), total, page, take);
    }
    public async Task<StudentDetailDto> GetAsync(string id, CancellationToken ct = default)
    {
        var x = await students.GetTrackedAsync(id, ct) ?? throw new NotFoundException("Student not found.");
        await access.RequireAsync("students.view", x.DepartmentId, ct);
        return StudentDetailMapper.Map(x);
    }
    public async Task UpdateAsync(StudentEditInput input, CancellationToken ct = default)
    {
        // Approval-consumption checks, changes, and the paper-reference audit share
        // one serializable transaction to prevent conflicting uses of a reference.
        await work.ExecuteTransactionAsync(async token =>
        {
            var x = await students.GetTrackedAsync(input.StudentId, token) ?? throw new NotFoundException();
            await access.RequireAsync("students.edit", x.DepartmentId, token);
            StudentValidation.Ensure(input.FullName, input.DateOfBirth, input.Gender,
                input.GuardianName, input.GuardianPhone, input.Phone, input.Address, clock.Today);
            bool coreChanged = x.FullName != input.FullName.Trim() || x.DateOfBirth.Date != input.DateOfBirth.Date ||
                x.Gender != input.Gender.Trim() || x.GuardianName != input.GuardianName.Trim() ||
                x.GuardianPhone != input.GuardianPhone.Trim();
            ApprovalRecord? approval = null;
            if (coreChanged)
                approval = await approvals.RequireAsync(input.ApprovalReference ?? "", "StudentEdit", x.DepartmentId,
                    current.UserId ?? throw new ForbiddenException(), false, token);
            if (approval is not null)
                await audit.AddAsync(new AuditEntry { UserId = current.UserId,
                    Action = "ApprovedStudentEdit", EntityName = "Student", EntityId = x.StudentId,
                    NewValue = "Approval reference: " + approval.ReferenceNo, LoggedAt = clock.Now }, token);
            x.FullName = input.FullName.Trim(); x.DateOfBirth = input.DateOfBirth.Date;
            x.Gender = input.Gender.Trim(); x.GuardianName = input.GuardianName.Trim();
            x.GuardianPhone = input.GuardianPhone.Trim(); x.Phone = input.Phone.Trim();
            x.Address = input.Address.Trim();
        }, ct);
    }
}
public static class StudentDetailMapper
{
    public static StudentDetailDto Map(UA.Entities.Student x) => new(x.StudentId, x.UserId, x.FullName,
        x.DepartmentId, x.Department.Code, x.AdmissionBatchId, x.CurrentBatchId, x.SectionId,
        x.Section.Name, x.RollNo, x.DateOfBirth, x.Gender, x.GuardianName, x.GuardianPhone,
        x.Phone, x.Address, !string.IsNullOrEmpty(x.PhotoPath), x.Status,
        x.StudentStatusHistories.OrderByDescending(h => h.EffectiveDate).ThenByDescending(h => h.HistoryId)
            .Select(h => new StudentStatusItem(h.Status, h.EffectiveDate, h.Reason, h.Approval?.ReferenceNo, h.ChangedBy)).ToArray(),
        x.StudentBatchHistories.OrderByDescending(b => b.FromDate).ThenByDescending(b => b.MoveId)
            .Select(b => new StudentPlacementItem(b.BatchId, b.Batch.AdmissionYear, b.SectionId,
                b.Section.Name, b.FromDate, b.ToDate, b.Reason, b.Approval?.ReferenceNo)).ToArray(),
        x.UploadedDocuments.OrderByDescending(d => d.UploadedAt)
            .Select(d => new StudentDocumentItem(d.DocumentId, d.DocumentType, d.MimeType, d.UploadedAt)).ToArray(),
        x.Application?.Applicant is Applicant a
            ? new StudentPreviousEducationDto(a.SscBoard, a.SscRoll, a.SscResult,
                a.HscBoard, a.HscRoll, a.HscResult) : null);
}
public static class StudentValidation
{
    public static void Ensure(string name, DateTime birth, string gender, string guardian,
        string guardianPhone, string phone, string address, DateTime? today = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100 ||
            birth.Date < new DateTime(1900,1,1) || birth.Date >= (today ?? DateTime.Today).Date ||
            string.IsNullOrWhiteSpace(gender) || gender.Trim().Length > 10 ||
            string.IsNullOrWhiteSpace(guardian) || guardian.Trim().Length > 100 ||
            string.IsNullOrWhiteSpace(guardianPhone) || guardianPhone.Trim().Length > 20 ||
            string.IsNullOrWhiteSpace(phone) || phone.Trim().Length > 20 ||
            string.IsNullOrWhiteSpace(address) || address.Trim().Length > 255)
            throw new BusinessRuleException("Provide valid required student identity and contact information.");
    }
}
