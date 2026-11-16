namespace UA.Entities;

/// <summary>Mark components and maximum marks per department and course type.</summary>
/// <remarks>Requirements: FR-EXM-13, BR-11</remarks>
[Table("Mark_Component")]
[Index(nameof(DepartmentId), nameof(CourseType), nameof(Name), nameof(VersionNo), IsUnique = true, Name = "UQ_Mark_Component_department_id_course_type_name_version_no")]
public class MarkComponent
{
    /// <summary>Component identifier</summary>
    [Key]
    [Column("component_id")]
    public int ComponentId { get; set; }

    /// <summary>Department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Course type the scheme applies to</summary>
    [Required, MaxLength(15)]
    [Column("course_type")]
    public string CourseType { get; set; } = string.Empty;

    /// <summary>Component name (for example Midterm)</summary>
    [Required, MaxLength(40)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Maximum marks (all components sum to 100)</summary>
    [Column("max_marks", TypeName = "decimal(5,2)")]
    public decimal MaxMarks { get; set; }

    /// <summary>Marks come from the attendance scale</summary>
    [Column("is_attendance_based")]
    public bool IsAttendanceBased { get; set; }

    /// <summary>Scheme version</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>Date from which the version applies</summary>
    [Column("effective_from", TypeName = "date")]
    public DateTime EffectiveFrom { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [InverseProperty(nameof(MarkEntry.Component))]
    public virtual ICollection<MarkEntry> MarkEntries { get; set; } = new List<MarkEntry>();
}
