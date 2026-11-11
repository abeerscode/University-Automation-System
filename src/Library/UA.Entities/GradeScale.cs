namespace UniversityAutomation.Models;

/// <summary>Grading scale of a department.</summary>
/// <remarks>Requirements: FR-EXM-24, BR-08</remarks>
[Table("Grade_Scale")]
[Index(nameof(DepartmentId), nameof(VersionNo), nameof(LetterGrade), IsUnique = true, Name = "UQ_Grade_Scale_department_id_version_no_letter_grade")]
public class GradeScale
{
    /// <summary>Grade row identifier</summary>
    [Key]
    [Column("scale_id")]
    public int ScaleId { get; set; }

    /// <summary>Department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Scale version</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>Letter grade</summary>
    [Required, MaxLength(3)]
    [Column("letter_grade")]
    public string LetterGrade { get; set; } = string.Empty;

    /// <summary>Lower bound (inclusive)</summary>
    [Column("min_mark", TypeName = "decimal(5,2)")]
    public decimal MinMark { get; set; }

    /// <summary>Upper bound (exclusive; 100.00 inclusive for A+)</summary>
    [Column("max_mark", TypeName = "decimal(5,2)")]
    public decimal MaxMark { get; set; }

    /// <summary>Grade point</summary>
    [Column("grade_point", TypeName = "decimal(3,2)")]
    public decimal GradePoint { get; set; }

    /// <summary>Date from which the version applies</summary>
    [Column("effective_from", TypeName = "date")]
    public DateTime EffectiveFrom { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
}
