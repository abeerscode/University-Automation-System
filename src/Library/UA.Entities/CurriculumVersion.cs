namespace UA.Entities;

/// <summary>Versioned curriculum of a program.</summary>
/// <remarks>Requirements: FR-ACA-02, FR-ACA-03, A-04</remarks>
[Table("Curriculum_Version")]
[Index(nameof(ProgramId), nameof(VersionNo), IsUnique = true, Name = "UQ_Curriculum_Version_program_id_version_no")]
public class CurriculumVersion
{
    /// <summary>Curriculum version identifier</summary>
    [Key]
    [Column("curriculum_id")]
    public int CurriculumId { get; set; }

    /// <summary>Program</summary>
    [Column("program_id")]
    public int ProgramId { get; set; }

    /// <summary>Version number (unique within the program)</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>First admission year it applies to</summary>
    [Column("effective_from_year")]
    public short EffectiveFromYear { get; set; }

    /// <summary>Draft, Active or Retired</summary>
    [Required, MaxLength(10)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Approval reference</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    // Navigation
    [ForeignKey(nameof(ProgramId))]
    public virtual AcademicProgram Program { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
    [InverseProperty(nameof(CurriculumCourse.Curriculum))]
    public virtual ICollection<CurriculumCourse> CurriculumCourses { get; set; } = new List<CurriculumCourse>();
    [InverseProperty(nameof(Batch.Curriculum))]
    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
}
