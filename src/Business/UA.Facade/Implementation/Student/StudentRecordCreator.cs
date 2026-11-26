using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Student;
using UA.DataAccess.Interface.Administration;
using UA.Entities;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Integration;
using UA.Facade.Interface.Student;
using UA.Facade.Rules.Student;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Student;
/// <summary>Single reusable, transaction-participating student/account creation path.</summary>
public sealed class StudentRecordCreator(IStudentRepository students, IUserAccountRepository users,
    IRoleRepository roles, IRepository<UserRole> userRoles, IRepository<Department> departments,
    IRepository<Batch> batches, IRepository<Section> sections,
    IRepository<StudentStatusHistory> histories, IRepository<StudentBatchHistory> placements,
    IPasswordHasher passwords, ISecureTokenGenerator random, IStudentIdGenerator ids,
    IClock clock) : IStudentRecordCreator
{
    public async Task<StudentCreatedCredential> AddAsync(StudentRegistrationInput input, int changedBy,
        CancellationToken ct = default)
    {
        StudentValidation.Ensure(input.FullName, input.DateOfBirth, input.Gender,
            input.GuardianName, input.GuardianPhone, input.Phone, input.Address, clock.Today);
        var dept = await departments.FirstAsync(x => x.DepartmentId == input.DepartmentId && x.Status == "Active", ct)
            ?? throw new BusinessRuleException("Department is missing or inactive.");
        var batch = await batches.FirstAsync(x => x.BatchId == input.AdmissionBatchId && x.DepartmentId == dept.DepartmentId, ct)
            ?? throw new BusinessRuleException("Admission batch does not match the department.");
        var section = await sections.FirstAsync(x => x.SectionId == input.SectionId && x.BatchId == batch.BatchId, ct)
            ?? throw new BusinessRuleException("Section does not belong to the batch.");
        var id = ids.Generate(batch.AdmissionYear, dept.IdCode, input.RollNo);
        if (input.RollNo > batch.SeatLimit) throw new BusinessRuleException("Roll exceeds this batch's seat limit.");
        if (await students.GetTrackedAsync(id, ct) is not null ||
            await users.FirstAsync(x => x.Username == id, ct) is not null)
            throw new BusinessRuleException("Student ID or login already exists: " + id);
        // The account belongs to the student, with the normal student role. No default credentials are shared.
        var role = await roles.FirstAsync(x => x.Name == RoleNames.Student, ct)
            ?? throw new BusinessRuleException("Student role has not been seeded.");
        var email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        if (email is not null && await users.FirstAsync(x => x.Email == email, ct) is not null)
            throw new BusinessRuleException("Email already belongs to another account.");
        string temporaryPassword = "Ua" + random.GenerateToken()[..22] + "8";
        var account = new UserAccount { Username = id, Email = email,
            Mobile = input.Phone.Trim(), PasswordHash = passwords.Hash(temporaryPassword),
            IsVerified = false, Status = "Active" };
        var student = new UA.Entities.Student
        {
            StudentId = id, User = account, ApplicationId = input.ApplicationId,
            DepartmentId = dept.DepartmentId, AdmissionBatchId = batch.BatchId,
            CurrentBatchId = batch.BatchId, SectionId = section.SectionId, RollNo = input.RollNo,
            FullName = input.FullName.Trim(), DateOfBirth = input.DateOfBirth.Date,
            Gender = input.Gender.Trim(), GuardianName = input.GuardianName.Trim(),
            GuardianPhone = input.GuardianPhone.Trim(), Phone = input.Phone.Trim(),
            Address = input.Address.Trim(), PhotoPath = "", Status = "Active"
        };
        await users.AddAsync(account, ct);
        await userRoles.AddAsync(new UserRole { User = account, RoleId = role.RoleId }, ct);
        await students.AddAsync(student, ct);
        var initialDate = new DateTime(batch.AdmissionYear, 1, 1);
        await histories.AddAsync(new StudentStatusHistory { Student = student, Status = "Active",
            EffectiveDate = initialDate, Reason = "Initial enrollment/import", ChangedBy = changedBy }, ct);
        await placements.AddAsync(new StudentBatchHistory { Student = student, BatchId = batch.BatchId,
            SectionId = section.SectionId, FromDate = initialDate, Reason = "Initial enrollment/import" }, ct);
        return new StudentCreatedCredential(id, temporaryPassword);
    }
}
