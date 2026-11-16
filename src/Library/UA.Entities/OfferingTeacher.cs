namespace UA.Entities;

/// <summary>Assignment of teachers to offerings, with history.</summary>
/// <remarks>Requirements: FR-FAC-04, FR-FAC-06, A-07, A-14</remarks>
[Table("Offering_Teacher")]
[PrimaryKey(nameof(OfferingId), nameof(FacultyId))]
public class OfferingTeacher
{
    /// <summary>Offering</summary>
    [Column("offering_id")]
    public int OfferingId { get; set; }

    /// <summary>Teacher</summary>
    [Column("faculty_id")]
    public int FacultyId { get; set; }

    /// <summary>Assignment date</summary>
    [Column("assigned_on", TypeName = "date")]
    public DateTime AssignedOn { get; set; }

    /// <summary>End of the assignment</summary>
    [Column("ended_on", TypeName = "date")]
    public DateTime? EndedOn { get; set; }

    /// <summary>Designated to enter retake marks</summary>
    [Column("is_retake_marker")]
    public bool IsRetakeMarker { get; set; }

    // Navigation
    [ForeignKey(nameof(OfferingId))]
    public virtual Offering Offering { get; set; } = null!;
    [ForeignKey(nameof(FacultyId))]
    public virtual Faculty Faculty { get; set; } = null!;
}
