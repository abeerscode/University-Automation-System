namespace UA.Entities;

/// <summary>Master record of an enrolled student: identity, batch, section and status.</summary>
/// <remarks>Requirements: FR-STU-01 to FR-STU-05, FR-STU-12</remarks>
[Table("Student")]
[Index(nameof(ApplicationId), IsUnique = true, Name = "UQ_Student_application_id")]
[Index(nameof(UserId), IsUnique = true, Name = "UQ_Student_user_id")]
[Index(nameof(AdmissionBatchId), nameof(RollNo), IsUnique = true, Name = "UQ_Student_admission_batch_id_roll_no")]
public class Student
{
    /// <summary>Permanent student ID (BR-04)</summary>
    [Key]
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Application the student was admitted through</summary>
    [Column("application_id")]
    public int? ApplicationId { get; set; }

    /// <summary>Login account</summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Original admission batch (never changes)</summary>
    [Column("admission_batch_id")]
    public int AdmissionBatchId { get; set; }

    /// <summary>Batch the student currently belongs to</summary>
    [Column("current_batch_id")]
    public int CurrentBatchId { get; set; }

    /// <summary>Current section</summary>
    [Column("section_id")]
    public int SectionId { get; set; }

    /// <summary>Roll number (merit position at enrollment)</summary>
    [Column("roll_no")]
    public short RollNo { get; set; }

    /// <summary>Full name</summary>
    [Required, MaxLength(100)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Date of birth</summary>
    [Column("date_of_birth", TypeName = "date")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>Gender</summary>
    [Required, MaxLength(10)]
    [Column("gender")]
    public string Gender { get; set; } = string.Empty;

    /// <summary>Guardian name</summary>
    [Required, MaxLength(100)]
    [Column("guardian_name")]
    public string GuardianName { get; set; } = string.Empty;

    /// <summary>Guardian phone number</summary>
    [Required, MaxLength(20)]
    [Column("guardian_phone")]
    public string GuardianPhone { get; set; } = string.Empty;

    /// <summary>Student phone number</summary>
    [Required, MaxLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Postal address</summary>
    [Required, MaxLength(255)]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>Path of the student photo</summary>
    [Required, MaxLength(255)]
    [Column("photo_path")]
    public string PhotoPath { get; set; } = string.Empty;

    /// <summary>Active, Dropped, Graduated, Withdrawn, Expelled, Suspended or Alumni</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(ApplicationId))]
    public virtual Application? Application { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual UserAccount User { get; set; } = null!;
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [ForeignKey(nameof(AdmissionBatchId))]
    public virtual Batch AdmissionBatch { get; set; } = null!;
    [ForeignKey(nameof(CurrentBatchId))]
    public virtual Batch CurrentBatch { get; set; } = null!;
    [ForeignKey(nameof(SectionId))]
    public virtual Section Section { get; set; } = null!;
    [InverseProperty(nameof(StudentStatusHistory.Student))]
    public virtual ICollection<StudentStatusHistory> StudentStatusHistories { get; set; } = new List<StudentStatusHistory>();
    [InverseProperty(nameof(StudentBatchHistory.Student))]
    public virtual ICollection<StudentBatchHistory> StudentBatchHistories { get; set; } = new List<StudentBatchHistory>();
    [InverseProperty(nameof(SemesterDrop.Student))]
    public virtual SemesterDrop? SemesterDrop { get; set; }
    [InverseProperty(nameof(CourseEquivalence.Student))]
    public virtual ICollection<CourseEquivalence> CourseEquivalences { get; set; } = new List<CourseEquivalence>();
    [InverseProperty(nameof(DocumentRequest.Student))]
    public virtual ICollection<DocumentRequest> DocumentRequests { get; set; } = new List<DocumentRequest>();
    [InverseProperty(nameof(Enrollment.Student))]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    [InverseProperty(nameof(EligibilityHold.Student))]
    public virtual ICollection<EligibilityHold> EligibilityHolds { get; set; } = new List<EligibilityHold>();
    [InverseProperty(nameof(SeatAllocation.Student))]
    public virtual ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
    [InverseProperty(nameof(SemesterResult.Student))]
    public virtual ICollection<SemesterResult> SemesterResults { get; set; } = new List<SemesterResult>();
    [InverseProperty(nameof(Due.Student))]
    public virtual ICollection<Due> Dues { get; set; } = new List<Due>();
    [InverseProperty(nameof(Payment.Student))]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    [InverseProperty(nameof(FeeWaiver.Student))]
    public virtual ICollection<FeeWaiver> FeeWaivers { get; set; } = new List<FeeWaiver>();
    [InverseProperty(nameof(UploadedDocument.Student))]
    public virtual ICollection<UploadedDocument> UploadedDocuments { get; set; } = new List<UploadedDocument>();
}
