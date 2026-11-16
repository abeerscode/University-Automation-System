namespace UA.Entities;

/// <summary>Seat of a student in a sitting, including make-up days for confirmed clashes.</summary>
/// <remarks>Requirements: FR-EXM-04, FR-EXM-12</remarks>
[Table("Seat_Allocation")]
[Index(nameof(SittingId), nameof(StudentId), IsUnique = true, Name = "UQ_Seat_Allocation_sitting_id_student_id")]
public class SeatAllocation
{
    /// <summary>Seat identifier</summary>
    [Key]
    [Column("seat_id")]
    public int SeatId { get; set; }

    /// <summary>Sitting</summary>
    [Column("sitting_id")]
    public int SittingId { get; set; }

    /// <summary>Student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Exam hall</summary>
    [Column("room_id")]
    public int RoomId { get; set; }

    /// <summary>Seat number</summary>
    [Required, MaxLength(10)]
    [Column("seat_no")]
    public string SeatNo { get; set; } = string.Empty;

    /// <summary>Make-up day for a confirmed clash</summary>
    [Column("makeup_day", TypeName = "date")]
    public DateTime? MakeupDay { get; set; }

    /// <summary>Department user who confirmed the clash</summary>
    [Column("clash_confirmed_by")]
    public int? ClashConfirmedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(SittingId))]
    public virtual ExamSitting Sitting { get; set; } = null!;
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(RoomId))]
    public virtual Room Room { get; set; } = null!;
    [ForeignKey(nameof(ClashConfirmedBy))]
    public virtual UserAccount? ClashConfirmedByUser { get; set; }
}
