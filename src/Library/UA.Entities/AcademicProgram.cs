namespace UA.Entities;

/// <summary>The single bachelor program of a department.</summary>
/// <remarks>Requirements: BR-02, FR-ACA-01</remarks>
// Table name stays [Program]; the C# class is AcademicProgram to avoid clashing
// with the compiler-generated Program class.
[Table("Program")]
[Index(nameof(DepartmentId), IsUnique = true, Name = "UQ_Program_department_id")]
public class AcademicProgram
{
    /// <summary>Program identifier</summary>
    [Key]
    [Column("program_id")]
    public int ProgramId { get; set; }

    /// <summary>Owning department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Program name</summary>
    [Required, MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Total credits of the program</summary>
    [Column("total_credits")]
    public short TotalCredits { get; set; }

    /// <summary>Number of semesters</summary>
    [Column("semester_count")]
    public byte SemesterCount { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [InverseProperty(nameof(CurriculumVersion.Program))]
    public virtual ICollection<CurriculumVersion> CurriculumVersions { get; set; } = new List<CurriculumVersion>();
}
