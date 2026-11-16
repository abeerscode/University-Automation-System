namespace UA.Entities;

/// <summary>Seats offered by each department in a circular.</summary>
/// <remarks>Requirements: FR-ADM-01, FR-ADM-09</remarks>
[Table("Circular_Seat")]
[PrimaryKey(nameof(CircularId), nameof(DepartmentId))]
public class CircularSeat
{
    /// <summary>Circular</summary>
    [Column("circular_id")]
    public int CircularId { get; set; }

    /// <summary>Department offering the seats</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Number of seats offered</summary>
    [Column("seats")]
    public short Seats { get; set; }

    // Navigation
    [ForeignKey(nameof(CircularId))]
    public virtual AdmissionCircular Circular { get; set; } = null!;
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
}
