using System.ComponentModel.DataAnnotations;
namespace UA.Web.UI.Areas.Academic.Models;
public sealed class DepartmentEditForm
{
    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 3)] public string Name { get; set; } = "";
    [Required] public string Status { get; set; } = "Active";
}
public sealed class ProgramForm
{
    public int DepartmentId { get; set; }
    [Required, StringLength(100, MinimumLength = 3)] public string Name { get; set; } = "";
    [Range(96, 144)] public short TotalCredits { get; set; } = 120;
}
public sealed class SettingsForm
{
    public int DepartmentId { get; set; }
    public int TermId { get; set; }
    [Range(12, 18)] public int NormalCredits { get; set; } = 15;
    [Range(75, 100)] public decimal MinimumAttendance { get; set; } = 75;
    [Required] public string ClassStarts { get; set; } = "09:00";
    [Required] public string ClassEnds { get; set; } = "17:00";
    [Range(1, 10000)] public int SeatsPerIntake { get; set; } = 120;
    [Range(1, 1000)] public int SectionCapacity { get; set; } = 40;
}
public sealed class CourseForm
{
    public int Id { get; set; }
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Required, StringLength(15, MinimumLength = 2)] public string Code { get; set; } = "";
    [Required, StringLength(150, MinimumLength = 3)] public string Title { get; set; } = "";
    [Range(typeof(decimal), "0.1", "18")] public decimal Credits { get; set; } = 3;
    [Required] public string Type { get; set; } = "Theory";
    public int[] PrerequisiteIds { get; set; } = [];
}
public sealed class CurriculumCreateForm
{
    [Range(1, int.MaxValue)] public int ProgramId { get; set; }
    [Range(2000, 2100)] public short EffectiveYear { get; set; } = (short)DateTime.Today.Year;
}
public sealed class TermForm
{
    public int Id { get; set; }
    [Range(2000, 2100)] public short Year { get; set; } = (short)DateTime.Today.Year;
    [Range(1, 2)] public byte TermNo { get; set; } = 1;
    [DataType(DataType.Date)] public DateTime RegistrationDeadline { get; set; }
    [DataType(DataType.Date)] public DateTime DropDeadline { get; set; }
    [DataType(DataType.Date)] public DateTime RetakeDeadline { get; set; }
}
public sealed class CalendarEventForm
{
    public int Id { get; set; }
    public int TermId { get; set; }
    [Required] public string Type { get; set; } = "Class period";
    [Required, StringLength(100, MinimumLength = 3)] public string Title { get; set; } = "";
    [DataType(DataType.Date)] public DateTime Starts { get; set; }
    [DataType(DataType.Date)] public DateTime Ends { get; set; }
}
public sealed class BatchForm
{
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Range(2000, 2100)] public short Year { get; set; } = (short)DateTime.Today.Year;
    [Range(1, 10000)] public short Seats { get; set; } = 120;
    [Range(1, 1000)] public short SectionCapacity { get; set; } = 40;
}
public sealed class SectionForm
{
    public int BatchId { get; set; }
    [Required, StringLength(10)] public string Name { get; set; } = "";
    [Range(1, 1000)] public short Capacity { get; set; } = 40;
}
public sealed class RoomForm
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string Name { get; set; } = "";
    [Range(1, 1000)] public short Capacity { get; set; } = 40;
    [Required] public string Type { get; set; } = "Classroom";
}
