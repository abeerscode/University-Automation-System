namespace UA.Entities;

/// <summary>Bands that convert attendance percentage to attendance marks.</summary>
/// <remarks>Requirements: FR-ACA-13, FR-EXM-26, A-12</remarks>
[Table("Attendance_Scale")]
[Index(nameof(DepartmentId), nameof(VersionNo), nameof(MinPercent), IsUnique = true, Name = "UQ_Attendance_Scale_department_id_version_no_min_percent")]
public class AttendanceScale
{
    /// <summary>Band identifier</summary>
    [Key]
    [Column("scale_id")]
    public int ScaleId { get; set; }

    /// <summary>Department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Scale version</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>Lower bound of the band (inclusive)</summary>
    [Column("min_percent", TypeName = "decimal(5,2)")]
    public decimal MinPercent { get; set; }

    /// <summary>Share of the component maximum awarded</summary>
    [Column("mark_percent", TypeName = "decimal(5,2)")]
    public decimal MarkPercent { get; set; }

    /// <summary>Date from which the version applies</summary>
    [Column("effective_from", TypeName = "date")]
    public DateTime EffectiveFrom { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
}
