using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.DataAccess.Interface.Faculty;
namespace UA.DataAccess.Implementation.Faculty;
public sealed class FacultyRepository(UniversityDbContext db) : IFacultyRepository
{
    public async Task<(IReadOnlyList<UA.Entities.Faculty> Items, int Total)> SearchAsync(string? query,
        int[] departments, int? departmentId, string? type, string? status, int skip, int take, CancellationToken ct = default)
    {
        var q = db.Faculties.AsNoTracking().Where(x => departments.Contains(x.DepartmentId));
        if (!string.IsNullOrWhiteSpace(query)) { var s = query.Trim(); q = q.Where(x => x.FullName.Contains(s) || x.EmployeeRef.Contains(s)); }
        if (departmentId.HasValue) q = q.Where(x => x.DepartmentId == departmentId.Value);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.FacultyType == type);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        int total = await q.CountAsync(ct);
        var rows = await q.Include(x => x.Department).OrderBy(x => x.FullName).ThenBy(x => x.FacultyId)
            .Skip(skip).Take(take).ToListAsync(ct);
        return (rows, total);
    }
    public Task<bool> EmployeeRefExistsAsync(string reference, CancellationToken ct = default) =>
        db.Faculties.AnyAsync(x => x.EmployeeRef == reference, ct);
    public Task<UA.Entities.Faculty?> ByIdAsync(int id, CancellationToken ct = default) =>
        db.Faculties.Include(x => x.Department).Include(x => x.User).FirstOrDefaultAsync(x => x.FacultyId == id, ct);
    public Task<UA.Entities.Faculty?> ByAccountAsync(int userId, CancellationToken ct = default) =>
        db.Faculties.Include(x => x.Department).FirstOrDefaultAsync(x => x.UserId == userId, ct);
    public Task AddAsync(UA.Entities.Faculty profile, CancellationToken ct = default) => db.Faculties.AddAsync(profile, ct).AsTask();
}
