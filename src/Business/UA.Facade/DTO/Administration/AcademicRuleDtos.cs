namespace UA.Facade.DTO.Administration;
public sealed record GradeBandDto(string LetterGrade, decimal MinMark, decimal MaxMark, decimal GradePoint, int VersionNo);
public sealed record MarkPartDto(string CourseType, string Name, decimal MaxMarks, bool AttendanceBased, int VersionNo);
public sealed record AttendanceBandDto(decimal MinPercent, decimal MarkPercent, int VersionNo);
