using UA.Entities;
using StudentEntity = UA.Entities.Student;
namespace UA.DataAccess.Interface.Student;
public interface IStudentRepository
{
    Task<(IReadOnlyList<StudentEntity> Items, int Total)> SearchAsync(string? query, IReadOnlyCollection<int> allowedDepartments,
        int? departmentId, int? batchId, int? sectionId, string? status, int skip, int take, CancellationToken ct = default);
    Task<StudentEntity?> GetTrackedAsync(string id, CancellationToken ct = default);
    Task<StudentEntity?> ByAccountAsync(int userId, CancellationToken ct = default);
    Task AddAsync(StudentEntity student, CancellationToken ct = default);
}
