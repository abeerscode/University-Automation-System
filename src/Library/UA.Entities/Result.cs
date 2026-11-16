namespace UA.Entities;

/// <summary>Versioned course result of an enrollment.</summary>
/// <remarks>Requirements: FR-EXM-17 to FR-EXM-21, BR-18</remarks>
[Table("Result")]
[Index(nameof(EnrollmentId), nameof(VersionNo), IsUnique = true, Name = "UQ_Result_enrollment_id_version_no")]
public class Result
{
    /// <summary>Result identifier</summary>
    [Key]
    [Column("result_id")]
    public int ResultId { get; set; }

    /// <summary>Enrollment</summary>
    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    /// <summary>Version number (unique within the enrollment)</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>Total marks</summary>
    [Column("total_marks", TypeName = "decimal(5,2)")]
    public decimal TotalMarks { get; set; }

    /// <summary>Letter grade</summary>
    [Required, MaxLength(3)]
    [Column("letter_grade")]
    public string LetterGrade { get; set; } = string.Empty;

    /// <summary>Grade point</summary>
    [Column("grade_point", TypeName = "decimal(3,2)")]
    public decimal GradePoint { get; set; }

    /// <summary>Normal, Absent, Withheld or Expelled</summary>
    [Required, MaxLength(10)]
    [Column("result_status")]
    public string ResultStatus { get; set; } = string.Empty;

    /// <summary>Replaced by a retake, marked (R)</summary>
    [Column("is_replaced")]
    public bool IsReplaced { get; set; }

    /// <summary>Grade scale version used</summary>
    [Column("grade_scale_version")]
    public int GradeScaleVersion { get; set; }

    /// <summary>Attendance scale version used</summary>
    [Column("attendance_scale_version")]
    public int AttendanceScaleVersion { get; set; }

    /// <summary>Publication time</summary>
    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    /// <summary>Result version it corrects</summary>
    [Column("supersedes_result_id")]
    public int? SupersedesResultId { get; set; }

    /// <summary>Reason of a correction</summary>
    [MaxLength(255)]
    [Column("correction_reason")]
    public string? CorrectionReason { get; set; }

    /// <summary>Approval reference</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    // Navigation
    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;
    [ForeignKey(nameof(SupersedesResultId))]
    public virtual Result? SupersedesResult { get; set; }
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
    [InverseProperty(nameof(Result.SupersedesResult))]
    public virtual ICollection<Result> SupersedingResults { get; set; } = new List<Result>();
}
