using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.DataAccess.Interface.Faculty;
using UA.Entities;
namespace UA.DataAccess.Implementation.Faculty;
public sealed class MentorAssignmentRepository(UniversityDbContext db) : IMentorAssignmentRepository
{
    public async Task<IReadOnlyList<MentorAssignment>> ForDepartmentAsync(int departmentId, CancellationToken ct = default) =>
        await db.MentorAssignments.AsNoTracking().Where(a => a.Batch.DepartmentId == departmentId)
            .Include(a => a.Faculty).Include(a => a.Batch).Include(a => a.Section)
            .OrderByDescending(a => a.AssignedOn).ThenByDescending(a => a.AssignmentId).ToListAsync(ct);
    public async Task<IReadOnlyList<MentorAssignment>> ForFacultyAsync(int facultyId, CancellationToken ct = default) =>
        await db.MentorAssignments.AsNoTracking().Where(a => a.FacultyId == facultyId)
            .Include(a => a.Batch).Include(a => a.Section)
            .OrderByDescending(a => a.AssignedOn).ToListAsync(ct);
    public Task<MentorAssignment?> ActiveForTargetAsync(int batchId, int? sectionId, CancellationToken ct = default) =>
        db.MentorAssignments.FirstOrDefaultAsync(a => a.BatchId == batchId && a.SectionId == sectionId && a.EndedOn == null, ct);
    public Task<MentorAssignment?> ByIdAsync(int id, CancellationToken ct = default) =>
        db.MentorAssignments.Include(a => a.Batch).FirstOrDefaultAsync(a => a.AssignmentId == id, ct);
    public Task AddAsync(MentorAssignment x, CancellationToken ct = default) => db.MentorAssignments.AddAsync(x, ct).AsTask();
    public async Task<IReadOnlyList<UA.Entities.Student>> MenteesAsync(int batchId, int? sectionId, CancellationToken ct = default)
    {
        var q = db.Students.AsNoTracking().Where(s => s.CurrentBatchId == batchId);
        if (sectionId.HasValue) q = q.Where(s => s.SectionId == sectionId.Value);
        return await q.Include(s => s.Section).OrderBy(s => s.RollNo).ThenBy(s => s.StudentId).ToListAsync(ct);
    }
}
