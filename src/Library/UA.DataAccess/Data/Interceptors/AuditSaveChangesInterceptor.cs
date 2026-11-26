using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UA.Entities;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.DataAccess.Data.Interceptors;
public sealed class AuditSaveChangesInterceptor(ICurrentUser current, IClock clock) : SaveChangesInterceptor
{
    private static readonly HashSet<string> Sensitive = new(StringComparer.OrdinalIgnoreCase)
        { "PasswordHash", "CodeHash", "Mobile", "Email", "Username" };
    private static readonly HashSet<Type> Audited =
        [typeof(UserAccount), typeof(UserRole), typeof(Role), typeof(RolePermission), typeof(ConfigParameter),
         typeof(AdmissionCircular), typeof(CircularSeat), typeof(Applicant), typeof(Application), typeof(WebPage), typeof(WebPageVersion), typeof(ApprovalRecord), typeof(Notice), typeof(GradeScale), typeof(MarkComponent),
         typeof(AttendanceScale), typeof(NotificationTemplate), typeof(Department), typeof(AcademicProgram),
         typeof(Course), typeof(CoursePrerequisite), typeof(CurriculumVersion), typeof(CurriculumCourse),
         typeof(AcademicTerm), typeof(AcademicCalendarEvent), typeof(Batch), typeof(Section), typeof(Room), typeof(Student), typeof(StudentStatusHistory), typeof(StudentBatchHistory),
         typeof(Faculty), typeof(MentorAssignment), typeof(OfferingTeacher), typeof(Offering), typeof(ClassRoutine), typeof(AttendanceRecord),
         typeof(SemesterDrop), typeof(CourseEquivalence), typeof(DocumentRequest), typeof(Enrollment), typeof(Result), typeof(ExamEvent), typeof(ExamSitting), typeof(SeatAllocation), typeof(SemesterResult), typeof(MarkEntry), typeof(MarkSubmission),
         typeof(Payment), typeof(Due), typeof(FeeWaiver), typeof(FeeType), typeof(PaymentAllocation), typeof(Receipt), typeof(EligibilityHold), typeof(UploadedDocument)];
    public override InterceptionResult<int> SavingChanges(DbContextEventData data, InterceptionResult<int> result)
    { Track(data.Context); return base.SavingChanges(data, result); }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    { Track(data.Context); return base.SavingChangesAsync(data, result, cancellationToken); }
    private void Track(DbContext? db)
    {
        if (db is null) return;
        db.ChangeTracker.DetectChanges();
        var changes = db.ChangeTracker.Entries().Where(e => Audited.Contains(e.Entity.GetType()) &&
            e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
        foreach (var e in changes)
        {
            var before = new Dictionary<string, object?>();
            var after = new Dictionary<string, object?>();
            foreach (var p in e.Properties.Where(x => !x.Metadata.IsShadowProperty() && !Sensitive.Contains(x.Metadata.Name)))
            {
                if(e.Entity is Applicant && p.Metadata.Name is not ("ApplicantId" or "UserId")) continue;
                if (e.State != EntityState.Added) before[p.Metadata.Name] = p.OriginalValue;
                if (e.State != EntityState.Deleted) after[p.Metadata.Name] = p.CurrentValue;
            }
            var id = string.Join(",", e.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue?.ToString() ?? ""));
            db.Set<AuditEntry>().Add(new AuditEntry
            {
                UserId = current.UserId, Action = e.State.ToString(), EntityName = e.Metadata.ClrType.Name,
                EntityId = string.IsNullOrEmpty(id) ? "pending" : id,
                OldValue = before.Count == 0 ? null : JsonSerializer.Serialize(before),
                NewValue = after.Count == 0 ? null : JsonSerializer.Serialize(after), LoggedAt = clock.Now
            });
        }
    }
}
