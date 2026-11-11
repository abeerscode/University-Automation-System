namespace UniversityAutomation.Models;

/// <summary>Mentor assigned to a batch or section.</summary>
/// <remarks>Requirements: FR-FAC-09</remarks>
[Table("Mentor_Assignment")]
public class MentorAssignment
{
    /// <summary>Assignment identifier</summary>
    [Key]
    [Column("assignment_id")]
    public int AssignmentId { get; set; }

    /// <summary>Mentor</summary>
    [Column("faculty_id")]
    public int FacultyId { get; set; }

    /// <summary>Batch</summary>
    [Column("batch_id")]
    public int BatchId { get; set; }

    /// <summary>Section (empty for the whole batch)</summary>
    [Column("section_id")]
    public int? SectionId { get; set; }

    /// <summary>Assignment date</summary>
    [Column("assigned_on", TypeName = "date")]
    public DateTime AssignedOn { get; set; }

    /// <summary>End of the assignment</summary>
    [Column("ended_on", TypeName = "date")]
    public DateTime? EndedOn { get; set; }

    // Navigation
    [ForeignKey(nameof(FacultyId))]
    public virtual Faculty Faculty { get; set; } = null!;
    [ForeignKey(nameof(BatchId))]
    public virtual Batch Batch { get; set; } = null!;
    [ForeignKey(nameof(SectionId))]
    public virtual Section? Section { get; set; }
}
