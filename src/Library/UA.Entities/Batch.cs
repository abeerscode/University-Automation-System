namespace UA.Entities;

/// <summary>Yearly intake of a department, linked to a curriculum version.</summary>
/// <remarks>Requirements: FR-ACA-05, A-04</remarks>
[Table("Batch")]
[Index(nameof(DepartmentId), nameof(AdmissionYear), IsUnique = true, Name = "UQ_Batch_department_id_admission_year")]
public class Batch
{
    /// <summary>Batch identifier</summary>
    [Key]
    [Column("batch_id")]
    public int BatchId { get; set; }

    /// <summary>Department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Curriculum version in force at admission</summary>
    [Column("curriculum_id")]
    public int CurriculumId { get; set; }

    /// <summary>Admission year (unique within the department)</summary>
    [Column("admission_year")]
    public short AdmissionYear { get; set; }

    /// <summary>Number of seats</summary>
    [Column("seat_limit")]
    public short SeatLimit { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [ForeignKey(nameof(CurriculumId))]
    public virtual CurriculumVersion Curriculum { get; set; } = null!;
    [InverseProperty(nameof(Student.AdmissionBatch))]
    public virtual ICollection<Student> AdmissionBatchStudents { get; set; } = new List<Student>();
    [InverseProperty(nameof(Student.CurrentBatch))]
    public virtual ICollection<Student> CurrentBatchStudents { get; set; } = new List<Student>();
    [InverseProperty(nameof(StudentBatchHistory.Batch))]
    public virtual ICollection<StudentBatchHistory> StudentBatchHistories { get; set; } = new List<StudentBatchHistory>();
    [InverseProperty(nameof(SemesterDrop.TargetBatch))]
    public virtual ICollection<SemesterDrop> TargetBatchSemesterDrops { get; set; } = new List<SemesterDrop>();
    [InverseProperty(nameof(Section.Batch))]
    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    [InverseProperty(nameof(Offering.Batch))]
    public virtual ICollection<Offering> Offerings { get; set; } = new List<Offering>();
    [InverseProperty(nameof(MentorAssignment.Batch))]
    public virtual ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
}
