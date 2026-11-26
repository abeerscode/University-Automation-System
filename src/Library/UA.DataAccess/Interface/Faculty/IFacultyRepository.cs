using UA.Entities;
namespace UA.DataAccess.Interface.Faculty;
public interface IFacultyRepository
{
    Task<(IReadOnlyList<UA.Entities.Faculty> Items, int Total)> SearchAsync(string? query, int[] departments,
        int? departmentId, string? type, string? status, int skip, int take, CancellationToken ct = default);
    Task<bool> EmployeeRefExistsAsync(string reference, CancellationToken ct = default);
    Task<UA.Entities.Faculty?> ByIdAsync(int id, CancellationToken ct = default);
    Task<UA.Entities.Faculty?> ByAccountAsync(int userId, CancellationToken ct = default);
    Task AddAsync(UA.Entities.Faculty profile, CancellationToken ct = default);
}
