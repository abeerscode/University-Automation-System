using UA.Entities;
namespace UA.DataAccess.Interface.Faculty;
public interface IMentorAssignmentRepository
{
    Task<IReadOnlyList<MentorAssignment>> ForDepartmentAsync(int departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<MentorAssignment>> ForFacultyAsync(int facultyId, CancellationToken ct = default);
    Task<MentorAssignment?> ActiveForTargetAsync(int batchId, int? sectionId, CancellationToken ct = default);
    Task<MentorAssignment?> ByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(MentorAssignment assignment, CancellationToken ct = default);
    Task<IReadOnlyList<UA.Entities.Student>> MenteesAsync(int batchId, int? sectionId, CancellationToken ct = default);
}
