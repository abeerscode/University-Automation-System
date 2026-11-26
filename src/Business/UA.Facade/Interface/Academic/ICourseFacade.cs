using UA.Facade.DTO.Academic;
namespace UA.Facade.Interface.Academic;
public interface ICourseFacade
{
Task<IReadOnlyList<CourseDto>> ListAsync(int? departmentId = null, CancellationToken ct = default);
Task<CourseDto> SaveAsync(CourseInput input, CancellationToken ct = default);
}
