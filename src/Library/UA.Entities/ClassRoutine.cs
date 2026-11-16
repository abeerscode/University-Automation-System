namespace UA.Entities;

/// <summary>Weekly class slots of an offering.</summary>
/// <remarks>Requirements: FR-ACA-10</remarks>
[Table("Class_Routine")]
public class ClassRoutine
{
    /// <summary>Routine identifier</summary>
    [Key]
    [Column("routine_id")]
    public int RoutineId { get; set; }

    /// <summary>Offering</summary>
    [Column("offering_id")]
    public int OfferingId { get; set; }

    /// <summary>Room</summary>
    [Column("room_id")]
    public int RoomId { get; set; }

    /// <summary>Day of the week (1 to 7)</summary>
    [Column("day_of_week")]
    public byte DayOfWeek { get; set; }

    /// <summary>Start time</summary>
    [Column("start_time")]
    public TimeSpan StartTime { get; set; }

    /// <summary>End time</summary>
    [Column("end_time")]
    public TimeSpan EndTime { get; set; }

    // Navigation
    [ForeignKey(nameof(OfferingId))]
    public virtual Offering Offering { get; set; } = null!;
    [ForeignKey(nameof(RoomId))]
    public virtual Room Room { get; set; } = null!;
    [InverseProperty(nameof(AttendanceRecord.Routine))]
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
