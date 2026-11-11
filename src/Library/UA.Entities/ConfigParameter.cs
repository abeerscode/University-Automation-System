namespace UniversityAutomation.Models;

/// <summary>Versioned department and university parameters.</summary>
/// <remarks>Requirements: SR-03, FR-SEC-07, FR-ACA-01</remarks>
[Table("Config_Parameter")]
[Index(nameof(DepartmentId), nameof(ParamKey), nameof(VersionNo), IsUnique = true, Name = "UQ_Config_Parameter_department_id_param_key_version_no")]
public class ConfigParameter
{
    /// <summary>Parameter identifier</summary>
    [Key]
    [Column("parameter_id")]
    public int ParameterId { get; set; }

    /// <summary>Department (empty for university-wide)</summary>
    [Column("department_id")]
    public int? DepartmentId { get; set; }

    /// <summary>Parameter name</summary>
    [Required, MaxLength(60)]
    [Column("param_key")]
    public string ParamKey { get; set; } = string.Empty;

    /// <summary>Value</summary>
    [Required, MaxLength(255)]
    [Column("param_value")]
    public string ParamValue { get; set; } = string.Empty;

    /// <summary>Version number</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>Academic term from which this parameter version applies</summary>
    [Column("effective_term_id")]
    public int EffectiveTermId { get; set; }

    /// <summary>User who changed it</summary>
    [Column("changed_by")]
    public int ChangedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }
    [ForeignKey(nameof(EffectiveTermId))]
    public virtual AcademicTerm EffectiveTerm { get; set; } = null!;
    [ForeignKey(nameof(ChangedBy))]
    public virtual UserAccount ChangedByUser { get; set; } = null!;
}
