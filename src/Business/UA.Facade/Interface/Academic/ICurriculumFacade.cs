using UA.Facade.DTO.Academic;
namespace UA.Facade.Interface.Academic;
public interface ICurriculumFacade
{
Task<IReadOnlyList<CurriculumDto>> ListAsync(int? departmentId = null, CancellationToken ct = default);
Task<int> CreateDraftAsync(CurriculumInput input, CancellationToken ct = default);
Task SetCourseAsync(int curriculumId, int courseId, byte semester, CancellationToken ct = default);
Task RemoveCourseAsync(int curriculumId, int courseId, CancellationToken ct = default);
Task ApproveAsync(int curriculumId, int approvalId, CancellationToken ct = default);
}
