using Microsoft.EntityFrameworkCore;
using UA.Entities;
using UA.Framework.Constants;
using UA.Framework.Time;
namespace UA.DataAccess.Data.Seed;
/// <summary>Non-destructive, idempotent Phase-1 seed. Never executes migrations or overwrites existing rows.</summary>
public static class DbInitializer
{
    public static async Task SeedAsync(UniversityDbContext db, IClock clock, Func<string,string> hashPassword,
        string? adminName, string? adminPassword, string? adminEmail, CancellationToken ct = default)
    {
        var specs = new (string Name, bool Scoped)[] {
            (RoleNames.SystemAdministrator, false), (RoleNames.Registrar, false), (RoleNames.Controller, false),
            (RoleNames.Accountant, false), (RoleNames.Head, true), (RoleNames.Coordinator, true),
            (RoleNames.Faculty, true), (RoleNames.GuestFaculty, true), (RoleNames.Student, false), (RoleNames.Applicant, false),
            (RoleNames.Admission, false), (RoleNames.WebsiteEditor, false)
        };
        foreach (var (name, scoped) in specs)
            if (!await db.Roles.AnyAsync(r => r.Name == name, ct)) db.Roles.Add(new Role { Name = name, DepartmentScoped = scoped });
        await db.SaveChangesAsync(ct);
        var defined = new (string Name, string Code, string IdCode)[] {
            ("Computer Science and Engineering", "CSE", "01"), ("Business Administration", "BBA", "02"),
            ("Civil Engineering", "CE", "03"), ("English", "ENG", "04")
        };
        foreach (var d in defined)
            if (!await db.Departments.AnyAsync(x => x.IdCode == d.IdCode, ct))
                db.Departments.Add(new Department { Name = d.Name, Code = d.Code, IdCode = d.IdCode, Status = "Active" });
        await db.SaveChangesAsync(ct);
        int year = clock.Now.Year, termNo = clock.Now.Month <= 6 ? 1 : 2;
        var term = await db.AcademicTerms.FirstOrDefaultAsync(t => t.Year == year && t.TermNo == termNo, ct);
        if (term is null)
        {
            int firstMonth = termNo == 1 ? 1 : 7;
            var start = new DateTime(year, firstMonth, 1);
            term = new AcademicTerm { Year = (short)year, TermNo = (byte)termNo,
                StartDate = start, EndDate = start.AddMonths(6).AddDays(-1),
                RegistrationDeadline = start.AddDays(14), DropDeadline = start.AddDays(9), RetakeDeadline = start.AddMonths(1).AddDays(14) };
            db.AcademicTerms.Add(term); await db.SaveChangesAsync(ct);
        }
        var roleMap = await db.Roles.ToDictionaryAsync(x => x.Name, ct);
        foreach (var (name, codes) in DefaultGrants())
        {
            var roleId = roleMap[name].RoleId;
            var existing = (await db.RolePermissions.Where(x => x.RoleId == roleId).Select(x => x.PermissionCode).ToListAsync(ct)).ToHashSet();
            foreach (var code in codes.Where(c => !existing.Contains(c)))
                db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionCode = code });
        }
        await db.SaveChangesAsync(ct);
        // Examination rules are needed for Phase 7 even when no administrator password is supplied on an upgrade.
        var rulesStart = await db.AcademicTerms.MinAsync(t=>t.StartDate,ct);
        var departments = await db.Departments.ToListAsync(ct);
        var gradeRows = new (string Grade, decimal Min, decimal Max, decimal Points)[] {
            ("A+", 80, 100, 4.00m), ("A", 75, 80, 3.75m), ("A-", 70, 75, 3.50m),
            ("B+", 65, 70, 3.25m), ("B", 60, 65, 3.00m), ("B-", 55, 60, 2.75m),
            ("C+", 50, 55, 2.50m), ("C", 45, 50, 2.25m), ("D", 40, 45, 2.00m), ("F", 0, 40, 0.00m)
        };
        foreach (var d in departments)
        {
            if (!await db.GradeScales.AnyAsync(x => x.DepartmentId == d.DepartmentId, ct))
                foreach (var row in gradeRows) db.GradeScales.Add(new GradeScale { DepartmentId = d.DepartmentId,
                    LetterGrade = row.Grade, MinMark = row.Min, MaxMark = row.Max, GradePoint = row.Points,
                    VersionNo = 1, EffectiveFrom = rulesStart });
            if (!await db.MarkComponents.AnyAsync(x => x.DepartmentId == d.DepartmentId, ct))
            {
                foreach (var (course, label, max, attend) in new (string, string, decimal, bool)[] {
                    ("Theory", "Final", 50, false), ("Theory", "Midterm", 25, false),
                    ("Theory", "Attendance", 5, true), ("Theory", "Tests and Assignments", 20, false),
                    ("Lab", "Attendance", 20, true), ("Lab", "Report", 30, false), ("Lab", "Project", 50, false) })
                    db.MarkComponents.Add(new MarkComponent { DepartmentId = d.DepartmentId, CourseType = course,
                        Name = label, MaxMarks = max, IsAttendanceBased = attend, VersionNo = 1, EffectiveFrom = rulesStart });
            }
            if (!await db.MarkComponents.AnyAsync(x=>x.DepartmentId==d.DepartmentId&&x.CourseType=="TheoryRetake",ct))
                db.MarkComponents.Add(new MarkComponent{DepartmentId=d.DepartmentId,CourseType="TheoryRetake",Name="Retake assessment",MaxMarks=100,VersionNo=1,EffectiveFrom=rulesStart});
            if (!await db.AttendanceScales.AnyAsync(x => x.DepartmentId == d.DepartmentId, ct))
                foreach (var (min, percent) in new (decimal, decimal)[] { (90, 100), (85, 80), (80, 60), (75, 40), (70, 20), (0, 0) })
                    db.AttendanceScales.Add(new AttendanceScale { DepartmentId = d.DepartmentId, VersionNo = 1,
                        MinPercent = min, MarkPercent = percent, EffectiveFrom = rulesStart });
        }
        foreach (var (code, subject, body) in new (string,string,string)[] {
            ("admission.submitted", "Application submitted", "View your application and outstanding invoice in the applicant portal."),
            ("admission.deadline", "Enrollment deadline approaching", "Check the current enrollment deadline and outstanding requirements in the applicant portal."),
            ("admission.result", "Admission result published", "Sign in to the applicant portal to view your result and selection status."),
            ("exam.routine", "Examination routine available", "View your eligible examination seats and holds in the portal."),
            ("exam.results", "Examination results published", "Your published results are available in the portal."),
            ("student.limit.warning","Seven-year limit warning","Review the Student Management lifecycle list for students in their seventh admission year."),
            ("student.rejoin.missing","Junior batch missing","Review the dropped students in Student Management; a junior batch required for rejoin is missing."),
            ("student.rejoin.ready","Rejoin registration window open","Review approved drops in Student Management and complete equivalence decisions and junior-section placement before registration closes.") })
            if (!await db.NotificationTemplates.AnyAsync(t=>t.Code==code,ct))
                db.NotificationTemplates.Add(new NotificationTemplate { Code=code,Channel="In-app",Subject=subject,Body=body,IsActive=true });
        await db.SaveChangesAsync(ct);
        // No default or hardcoded administrator password: intentionally require an environment supplied secret.
        if (string.IsNullOrWhiteSpace(adminPassword)) return;
        var username = string.IsNullOrWhiteSpace(adminName) ? "sysadmin" : adminName;
        var rootRole = roleMap[RoleNames.SystemAdministrator].RoleId;
        int existingAdminId = await db.UserRoles.Where(x => x.RoleId == rootRole)
            .Select(x => x.UserId).FirstOrDefaultAsync(ct);
        UserAccount admin;
        if (existingAdminId != 0)
        {
            // Existing administrator is never modified or assigned a new password/role.
            admin = await db.UserAccounts.FirstAsync(x => x.UserId == existingAdminId, ct);
        }
        else
        {
            if (await db.UserAccounts.AnyAsync(x => x.Username == username, ct))
                throw new InvalidOperationException("Cannot bootstrap over an existing username; choose a new bootstrap username.");
            admin = new UserAccount { Username = username, PasswordHash = hashPassword(adminPassword),
                Email = adminEmail, IsVerified = true, Status = "Active" };
            db.UserAccounts.Add(admin); await db.SaveChangesAsync(ct);
            db.UserRoles.Add(new UserRole { UserId = admin.UserId, RoleId = rootRole }); await db.SaveChangesAsync(ct);
        }
        // Create default rules only once and only when an accountable administrator exists for changed_by FK.
        foreach (var (key, value) in new (string, string)[] {
            (ConfigKeys.AttendanceMinimum, "75"), (ConfigKeys.CreditsMinimum, "12"),
            (ConfigKeys.CreditsMaximum, "18"), (ConfigKeys.NormalCredits, "15"),
            (ConfigKeys.SectionCapacity, "40"), (ConfigKeys.SectionOverflow, "5"),
            (ConfigKeys.RetakeLimit, "2"), (ConfigKeys.GraduationYears, "7") })
        {
            if (!await db.ConfigParameters.AnyAsync(x => x.DepartmentId == null && x.ParamKey == key, ct))
                db.ConfigParameters.Add(new ConfigParameter { ParamKey = key, ParamValue = value, VersionNo = 1,
                    EffectiveTermId = term.TermId, ChangedBy = admin.UserId });
        }
        foreach (var (code, channel, subject, body) in new (string, string, string, string)[] {
            ("user.welcome", "Email", "Your UAS account", "Your account is ready."),
            ("notice.internal", "In-app", "University notice", "A notice was published."),
            ("user.verify", "Email", "UAS verification", "Please verify your account.") })
            if (!await db.NotificationTemplates.AnyAsync(t => t.Code == code, ct))
                db.NotificationTemplates.Add(new NotificationTemplate { Code = code, Channel = channel,
                    Subject = subject, Body = body, IsActive = true });
        await db.SaveChangesAsync(ct);
    }
    private static IEnumerable<(string Name, string[] Grants)> DefaultGrants()
    {
        yield return (RoleNames.SystemAdministrator, PermissionCodes.All);
        yield return (RoleNames.Registrar, ("audit.view approvals.record approvals.correct config.manage calendar.manage dashboards.view students.view status.change drop.approve graduation.process limit.decide requests.process reports.view exports.sensitive pages.approve notifications.view jobs.view").Split(' '));
        yield return (RoleNames.Controller, ("exams.manage routine.view seatplan.manage eligibility.view hold.release.dues marks.approve results.publish results.correct transcripts.issue makeup.manage notices.edit notices.approve dashboards.view notifications.view jobs.view").Split(' '));
        yield return (RoleNames.Accountant, ("fees.manage dues.view payments.record payments.reverse ledger.view reconciliation.view finance.reports waivers.manage hold.release.dues notifications.view jobs.view").Split(' '));
        yield return (RoleNames.Head, ("approvals.record students.view faculty.view faculty.manage mentors.manage workload.view courses.manage curriculum.manage reports.view exports.sensitive attendance.view marks.verify hold.release.attendance drop.recommend equivalence.decide retake.override departmentpage.edit pages.approve notices.edit notices.approve dashboards.view notifications.view jobs.view").Split(' '));
        yield return (RoleNames.Coordinator, ("students.view students.edit faculty.view faculty.manage mentors.manage workload.view courses.manage curriculum.manage section.change drop.request rejoin.place retake.enroll batches.manage offerings.manage registration.run reports.view exports.sensitive routine.manage assignments.manage attendance.view hold.release.attendance departmentpage.edit notices.edit notifications.view jobs.view").Split(' '));
        yield return (RoleNames.Faculty, ("attendance.view marks.enter routine.view notifications.view jobs.view").Split(' '));
        // Guest permissions deliberately exclude marks entry, management, exports, and jobs.
        yield return (RoleNames.GuestFaculty, ("attendance.view routine.view notifications.view").Split(' '));
        yield return (RoleNames.Student, ("notifications.view jobs.view").Split(' '));
        yield return (RoleNames.Applicant, ("notifications.view jobs.view").Split(' '));
        yield return (RoleNames.Admission, ("circulars.manage applications.review merit.manage enrollment.confirm enrollment.replace admission.reports notifications.view jobs.view").Split(' '));
        yield return (RoleNames.WebsiteEditor, ("pages.edit notices.edit departmentpage.edit notifications.view jobs.view").Split(' '));
    }
}
