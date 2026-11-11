namespace UniversityAutomation.Models;

/// <summary>Course offered to a batch and section in a term.</summary>
/// <remarks>Requirements: FR-ACA-06, BR-06</remarks>
[Table("Offering")]
[Index(nameof(TermId), nameof(SectionId), nameof(CourseId), IsUnique = true, Name = "UQ_Offering_term_id_section_id_course_id")]
public class Offering
{
    /// <summary>Offering identifier</summary>
    [Key]
    [Column("offering_id")]
    public int OfferingId { get; set; }

    /// <summary>Term</summary>
    [Column("term_id")]
    public int TermId { get; set; }

    /// <summary>Batch</summary>
    [Column("batch_id")]
    public int BatchId { get; set; }

    /// <summary>Section</summary>
    [Column("section_id")]
    public int SectionId { get; set; }

    /// <summary>Course</summary>
    [Column("course_id")]
    public int CourseId { get; set; }

    /// <summary>Semester number</summary>
    [Column("semester_no")]
    public byte SemesterNo { get; set; }

    /// <summary>Planned, Active or Closed</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(TermId))]
    public virtual AcademicTerm Term { get; set; } = null!;
    [ForeignKey(nameof(BatchId))]
    public virtual Batch Batch { get; set; } = null!;
    [ForeignKey(nameof(SectionId))]
    public virtual Section Section { get; set; } = null!;
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;
    [InverseProperty(nameof(ClassRoutine.Offering))]
    public virtual ICollection<ClassRoutine> ClassRoutines { get; set; } = new List<ClassRoutine>();
    [InverseProperty(nameof(Enrollment.Offering))]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    [InverseProperty(nameof(OfferingTeacher.Offering))]
    public virtual ICollection<OfferingTeacher> OfferingTeachers { get; set; } = new List<OfferingTeacher>();
    [InverseProperty(nameof(MarkSubmission.Offering))]
    public virtual ICollection<MarkSubmission> MarkSubmissions { get; set; } = new List<MarkSubmission>();
}
