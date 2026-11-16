namespace UA.Entities;

/// <summary>Section of a batch.</summary>
/// <remarks>Requirements: FR-ACA-05, BR-03, A-03</remarks>
[Table("Section")]
[Index(nameof(BatchId), nameof(Name), IsUnique = true, Name = "UQ_Section_batch_id_name")]
public class Section
{
    /// <summary>Section identifier</summary>
    [Key]
    [Column("section_id")]
    public int SectionId { get; set; }

    /// <summary>Batch</summary>
    [Column("batch_id")]
    public int BatchId { get; set; }

    /// <summary>Section name (unique within the batch)</summary>
    [Required, MaxLength(10)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Section capacity</summary>
    [Column("capacity")]
    public short Capacity { get; set; }

    // Navigation
    [ForeignKey(nameof(BatchId))]
    public virtual Batch Batch { get; set; } = null!;
    [InverseProperty(nameof(Student.Section))]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    [InverseProperty(nameof(StudentBatchHistory.Section))]
    public virtual ICollection<StudentBatchHistory> StudentBatchHistories { get; set; } = new List<StudentBatchHistory>();
    [InverseProperty(nameof(Offering.Section))]
    public virtual ICollection<Offering> Offerings { get; set; } = new List<Offering>();
    [InverseProperty(nameof(MentorAssignment.Section))]
    public virtual ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
}
