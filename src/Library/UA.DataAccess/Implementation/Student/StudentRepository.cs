using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.DataAccess.Interface.Student;
using UA.Entities;
using StudentEntity = UA.Entities.Student;
namespace UA.DataAccess.Implementation.Student;
public sealed class StudentRepository(UniversityDbContext db) : IStudentRepository
{
    public async Task<(IReadOnlyList<StudentEntity> Items, int Total)> SearchAsync(string? query,
        IReadOnlyCollection<int> allowedDepartments, int? departmentId, int? batchId,
        int? sectionId, string? status, int skip, int take, CancellationToken ct = default)
    {
        var ids = allowedDepartments.ToArray();
        var q = db.Students.AsNoTracking().Where(x => ids.Contains(x.DepartmentId));
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(x => x.StudentId.Contains(term) || x.FullName.Contains(term));
        }
        if (departmentId.HasValue) q = q.Where(x => x.DepartmentId == departmentId.Value);
        if (batchId.HasValue) q = q.Where(x => x.CurrentBatchId == batchId.Value || x.AdmissionBatchId == batchId.Value);
        if (sectionId.HasValue) q = q.Where(x => x.SectionId == sectionId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        var total = await q.CountAsync(ct);
        var data = await q.Include(x => x.Department).Include(x => x.CurrentBatch)
            .Include(x => x.AdmissionBatch).Include(x => x.Section)
            .OrderBy(x => x.StudentId).Skip(skip).Take(take).ToListAsync(ct);
        return (data, total);
    }
    public Task<StudentEntity?> GetTrackedAsync(string id, CancellationToken ct = default)
        => db.Students.Include(x => x.Department).Include(x => x.AdmissionBatch)
            .Include(x => x.CurrentBatch).Include(x => x.Section)
            .Include(x => x.StudentStatusHistories).ThenInclude(x => x.Approval)
            .Include(x => x.StudentBatchHistories).ThenInclude(x => x.Approval)
            .Include(x => x.StudentBatchHistories).ThenInclude(x => x.Batch)
            .Include(x => x.StudentBatchHistories).ThenInclude(x => x.Section)
            .Include(x => x.UploadedDocuments).Include(x => x.Application).ThenInclude(a => a!.Applicant)
            .AsSplitQuery().FirstOrDefaultAsync(x => x.StudentId == id, ct);
    public Task<StudentEntity?> ByAccountAsync(int userId, CancellationToken ct = default)
        => db.Students.FirstOrDefaultAsync(x => x.UserId == userId, ct);
    public Task AddAsync(StudentEntity student, CancellationToken ct = default) => db.Students.AddAsync(student, ct).AsTask();
}
