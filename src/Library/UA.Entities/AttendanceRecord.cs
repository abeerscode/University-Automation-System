namespace UniversityAutomation.Models;

/// <summary>Attendance of a student in one class.</summary>
/// <remarks>Requirements: FR-ACA-12</remarks>
[Table("Attendance_Record")]
[Index(nameof(EnrollmentId), nameof(RoutineId), nameof(ClassDate), IsUnique = true, Name = "UQ_Attendance_Record_enrollment_id_routine_id_class_date")]
public class AttendanceRecord
{
    /// <summary>Attendance identifier</summary>
    [Key]
    [Column("attendance_id")]
    public long AttendanceId { get; set; }

    /// <summary>Enrollment</summary>
    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    /// <summary>Class slot</summary>
    [Column("routine_id")]
    public int RoutineId { get; set; }

    /// <summary>Date of the class</summary>
    [Column("class_date", TypeName = "date")]
    public DateTime ClassDate { get; set; }

    /// <summary>Present or Absent</summary>
    [Required, MaxLength(10)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Teacher who recorded it</summary>
    [Column("recorded_by")]
    public int RecordedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;
    [ForeignKey(nameof(RoutineId))]
    public virtual ClassRoutine Routine { get; set; } = null!;
    [ForeignKey(nameof(RecordedBy))]
    public virtual UserAccount RecordedByUser { get; set; } = null!;
}
