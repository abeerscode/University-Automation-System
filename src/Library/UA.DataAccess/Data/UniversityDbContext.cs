using Microsoft.EntityFrameworkCore;
using UA.Entities;

namespace UA.DataAccess.Data;

/// <summary>
/// EF Core model for the redesigned University Automation schema.
/// Entity attributes define table/column mappings, keys, lengths and declared indexes.
/// This context supplies database-level rules that cannot be expressed by those
/// attributes: SQL Server delete behavior and cross-column ownership constraints.
/// </summary>
public sealed class UniversityDbContext : DbContext
{
    public UniversityDbContext(DbContextOptions<UniversityDbContext> options)
        : base(options)
    {
    }

    // Core & Identity

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    public DbSet<ConfigParameter> ConfigParameters => Set<ConfigParameter>();

    public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();

    // Organization & Academics

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Faculty> Faculties => Set<Faculty>();

    public DbSet<AcademicProgram> AcademicPrograms => Set<AcademicProgram>();

    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();

    public DbSet<AcademicCalendarEvent> AcademicCalendarEvents => Set<AcademicCalendarEvent>();

    public DbSet<Batch> Batches => Set<Batch>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CurriculumVersion> CurriculumVersions => Set<CurriculumVersion>();

    public DbSet<CurriculumCourse> CurriculumCourses => Set<CurriculumCourse>();

    public DbSet<CoursePrerequisite> CoursePrerequisites => Set<CoursePrerequisite>();

    public DbSet<CourseEquivalence> CourseEquivalences => Set<CourseEquivalence>();

    public DbSet<Offering> Offerings => Set<Offering>();

    public DbSet<OfferingTeacher> OfferingTeachers => Set<OfferingTeacher>();

    public DbSet<Section> Sections => Set<Section>();

    public DbSet<ClassRoutine> ClassRoutines => Set<ClassRoutine>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<MentorAssignment> MentorAssignments => Set<MentorAssignment>();

    // Students & Enrollment

    public DbSet<Student> Students => Set<Student>();

    public DbSet<StudentBatchHistory> StudentBatchHistories => Set<StudentBatchHistory>();

    public DbSet<StudentStatusHistory> StudentStatusHistories => Set<StudentStatusHistory>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<SemesterDrop> SemesterDrops => Set<SemesterDrop>();

    public DbSet<EligibilityHold> EligibilityHolds => Set<EligibilityHold>();

    public DbSet<AttendanceScale> AttendanceScales => Set<AttendanceScale>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    // Admission

    public DbSet<AdmissionCircular> AdmissionCirculars => Set<AdmissionCircular>();

    public DbSet<CircularSeat> CircularSeats => Set<CircularSeat>();

    public DbSet<Applicant> Applicants => Set<Applicant>();

    public DbSet<Application> Applications => Set<Application>();

    public DbSet<ApprovalRecord> ApprovalRecords => Set<ApprovalRecord>();

    public DbSet<DocumentRequest> DocumentRequests => Set<DocumentRequest>();

    public DbSet<UploadedDocument> UploadedDocuments => Set<UploadedDocument>();

    // Examinations & Results

    public DbSet<ExamEvent> ExamEvents => Set<ExamEvent>();

    public DbSet<ExamSitting> ExamSittings => Set<ExamSitting>();

    public DbSet<GradeScale> GradeScales => Set<GradeScale>();

    public DbSet<MarkComponent> MarkComponents => Set<MarkComponent>();

    public DbSet<MarkEntry> MarkEntries => Set<MarkEntry>();

    public DbSet<MarkSubmission> MarkSubmissions => Set<MarkSubmission>();

    public DbSet<Result> Results => Set<Result>();

    public DbSet<SemesterResult> SemesterResults => Set<SemesterResult>();

    // Finance

    public DbSet<FeeType> FeeTypes => Set<FeeType>();

    public DbSet<Due> Dues => Set<Due>();

    public DbSet<FeeWaiver> FeeWaivers => Set<FeeWaiver>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();

    public DbSet<Receipt> Receipts => Set<Receipt>();

    public DbSet<GatewayCallback> GatewayCallbacks => Set<GatewayCallback>();

    // Content & Operations

    public DbSet<Notice> Notices => Set<Notice>();

    public DbSet<SeatAllocation> SeatAllocations => Set<SeatAllocation>();

    public DbSet<WebPage> WebPages => Set<WebPage>();

    public DbSet<WebPageVersion> WebPageVersions => Set<WebPageVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SemesterDrop>().HasIndex(x=>x.StudentId,"UQ_Semester_Drop_student_id").IsUnique().HasFilter("[status] <> N'Rejected'");

        ConfigureRelationships(modelBuilder);
        ConfigureIntegrityConstraints(modelBuilder);
    }

    /// <summary>
    /// The entity classes declare their FK properties and navigation properties.
    /// EF Core therefore builds the actual relationships from the new model's
    /// [ForeignKey] and [InverseProperty] metadata. We explicitly select NO ACTION
    /// for every FK so SQL Server migrations do not create cascade paths or
    /// accidentally delete academic/financial history when a parent is removed.
    /// </summary>
    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }

    /// <summary>
    /// Business invariants documented by the new entity model. These checks are
    /// enforced by SQL Server, not merely by application validation.
    /// </summary>
    private static void ConfigureIntegrityConstraints(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Due>().ToTable("Due", table =>
        {
            table.HasCheckConstraint(
                "CK_Due_Owner",
                "[student_id] IS NOT NULL OR [applicant_id] IS NOT NULL");
        });

        modelBuilder.Entity<Payment>().ToTable("Payment", table =>
        {
            table.HasCheckConstraint(
                "CK_Payment_Owner",
                "[student_id] IS NOT NULL OR [applicant_id] IS NOT NULL");
        });

        modelBuilder.Entity<UploadedDocument>().ToTable("Uploaded_Document", table =>
        {
            table.HasCheckConstraint(
                "CK_Uploaded_Document_Owner",
                "[applicant_id] IS NOT NULL OR [student_id] IS NOT NULL");
        });
    }
}
