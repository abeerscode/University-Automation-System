using UA.Facade.DTO.Administration;
namespace UA.Facade.Interface.Administration;
public interface IAcademicRulesFacade
{
    Task<IReadOnlyList<GradeBandDto>> GradeBandsAsync(int departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<MarkPartDto>> MarkPartsAsync(int departmentId, string courseType, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceBandDto>> AttendanceBandsAsync(int departmentId, CancellationToken ct = default);
    Task<int> AddGradeVersionAsync(int departmentId, DateTime effectiveFrom, IReadOnlyList<GradeBandDto> bands, CancellationToken ct = default);
    Task<int> AddMarkSchemeVersionAsync(int departmentId, DateTime effectiveFrom, string courseType, IReadOnlyList<MarkPartDto> parts, CancellationToken ct = default);
    Task<int> AddAttendanceVersionAsync(int departmentId, DateTime effectiveFrom, IReadOnlyList<AttendanceBandDto> bands, CancellationToken ct = default);
}
