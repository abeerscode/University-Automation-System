using UA.Facade.DTO.Faculty;
using UA.Framework.Common;
namespace UA.Facade.Interface.Faculty;
public interface IFacultyFacade
{
    Task<PagedResult<FacultySummaryDto>> SearchAsync(FacultySearchInput filter, CancellationToken ct = default);
    Task<FacultyDetailDto> GetAsync(int facultyId, CancellationToken ct = default);
    Task<FacultyCreatedDto> CreateAsync(FacultySaveDto input, byte[]? photo, string? mime, CancellationToken ct = default);
    Task EditAsync(int facultyId, FacultySaveDto input, byte[]? photo, string? mime, CancellationToken ct = default);
    Task<FacultyPhotoDto> PhotoAsync(int facultyId, CancellationToken ct = default);
    Task<IReadOnlyList<FacultyWorkloadRowDto>> WorkloadAsync(int departmentId, int? termId, CancellationToken ct = default);
}
public interface IFacultyPortalFacade
{
    Task<FacultyDashboardDto> DashboardAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TeachingAssignmentDto>> CoursesAsync(int? termId = null, CancellationToken ct = default);
}
public interface ITeacherAssignmentFacade
{
    Task<IReadOnlyList<TeachingAssignmentDto>> OwnAsync(int? termId = null, CancellationToken ct = default);
}
public interface IMentorFacade
{
    Task<IReadOnlyList<MentorAssignmentDto>> ListAsync(int departmentId, CancellationToken ct = default);
    Task<MentorOptionsDto> OptionsAsync(int departmentId, CancellationToken ct = default);
    Task<int> AssignAsync(MentorSaveDto input, CancellationToken ct = default);
    Task EndAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MenteeSummaryDto>> OwnMenteesAsync(CancellationToken ct = default);
}
