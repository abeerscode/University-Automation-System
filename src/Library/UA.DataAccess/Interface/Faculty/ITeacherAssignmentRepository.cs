using UA.Entities;
namespace UA.DataAccess.Interface.Faculty;
public interface ITeacherAssignmentRepository
{
    Task<bool> IsAssignedAsync(int facultyId, int offeringId, DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<OfferingTeacher>> ForFacultyAsync(int facultyId, int? termId, CancellationToken ct = default);
    Task<IReadOnlyList<OfferingTeacher>> ForDepartmentAsync(int departmentId, int? termId, CancellationToken ct = default);
}
