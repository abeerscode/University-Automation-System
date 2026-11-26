using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.DataAccess.Interface.Faculty;
using UA.Entities;
namespace UA.DataAccess.Implementation.Faculty;
public sealed class TeacherAssignmentRepository(UniversityDbContext db) : ITeacherAssignmentRepository
{
    public Task<bool> IsAssignedAsync(int facultyId, int offeringId, DateTime today, CancellationToken ct = default) =>
        db.OfferingTeachers.AnyAsync(a => a.FacultyId == facultyId && a.OfferingId == offeringId &&
            a.AssignedOn <= today && (a.EndedOn == null || a.EndedOn > today) && a.Offering.Status != "Closed", ct);
    public async Task<IReadOnlyList<OfferingTeacher>> ForFacultyAsync(int facultyId, int? termId, CancellationToken ct = default)
    {
        var q = db.OfferingTeachers.AsNoTracking().Where(a => a.FacultyId == facultyId);
        if (termId.HasValue) q = q.Where(a => a.Offering.TermId == termId.Value);
        return await q.Include(a => a.Offering).ThenInclude(o => o.Course)
            .Include(a => a.Offering).ThenInclude(o => o.Term)
            .Include(a => a.Offering).ThenInclude(o => o.Section)
            .OrderByDescending(a => a.Offering.Term.StartDate).ThenBy(a => a.Offering.Course.Code).ToListAsync(ct);
    }
    public async Task<IReadOnlyList<OfferingTeacher>> ForDepartmentAsync(int departmentId, int? termId, CancellationToken ct = default)
    {
        var q = db.OfferingTeachers.AsNoTracking().Where(a => a.Faculty.DepartmentId == departmentId);
        if (termId.HasValue) q = q.Where(a => a.Offering.TermId == termId.Value);
        return await q.Include(a => a.Faculty).Include(a => a.Offering).ThenInclude(o => o.Course)
            .Include(a => a.Offering).ThenInclude(o => o.Term).ToListAsync(ct);
    }
}
