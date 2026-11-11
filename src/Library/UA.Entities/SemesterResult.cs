namespace UniversityAutomation.Models;

/// <summary>Published semester GPA and CGPA of a student.</summary>
/// <remarks>Requirements: FR-EXM-17, A-11</remarks>
[Table("Semester_Result")]
public class SemesterResult
{
    /// <summary>Identifier</summary>
    [Key]
    [Column("semester_result_id")]
    public int SemesterResultId { get; set; }

    /// <summary>Student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Term</summary>
    [Column("term_id")]
    public int TermId { get; set; }

    /// <summary>Semester number</summary>
    [Column("semester_no")]
    public byte SemesterNo { get; set; }

    /// <summary>Credits earned in the semester</summary>
    [Column("credits_earned", TypeName = "decimal(5,1)")]
    public decimal CreditsEarned { get; set; }

    /// <summary>Semester GPA</summary>
    [Column("semester_gpa", TypeName = "decimal(3,2)")]
    public decimal SemesterGpa { get; set; }

    /// <summary>CGPA at publication</summary>
    [Column("cgpa", TypeName = "decimal(3,2)")]
    public decimal Cgpa { get; set; }

    /// <summary>Version number</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>Publication time</summary>
    [Column("published_at")]
    public DateTime PublishedAt { get; set; }

    /// <summary>Version it corrects</summary>
    [Column("supersedes_id")]
    public int? SupersedesId { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(TermId))]
    public virtual AcademicTerm Term { get; set; } = null!;
    [ForeignKey(nameof(SupersedesId))]
    public virtual SemesterResult? Supersedes { get; set; }
    [InverseProperty(nameof(SemesterResult.Supersedes))]
    public virtual ICollection<SemesterResult> SupersedingSemesterResults { get; set; } = new List<SemesterResult>();
}
