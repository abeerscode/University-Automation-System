namespace UniversityAutomation.Models;

/// <summary>Classrooms, laboratories and exam halls.</summary>
/// <remarks>Requirements: FR-ACA-11</remarks>
[Table("Room")]
[Index(nameof(Name), IsUnique = true, Name = "UQ_Room_name")]
public class Room
{
    /// <summary>Room identifier</summary>
    [Key]
    [Column("room_id")]
    public int RoomId { get; set; }

    /// <summary>Room name or number</summary>
    [Required, MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Seating capacity</summary>
    [Column("capacity")]
    public short Capacity { get; set; }

    /// <summary>Classroom, Lab or Exam hall</summary>
    [Required, MaxLength(15)]
    [Column("room_type")]
    public string RoomType { get; set; } = string.Empty;

    // Navigation
    [InverseProperty(nameof(ClassRoutine.Room))]
    public virtual ICollection<ClassRoutine> ClassRoutines { get; set; } = new List<ClassRoutine>();
    [InverseProperty(nameof(SeatAllocation.Room))]
    public virtual ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
