using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using UA.Facade.DTO.Faculty;
namespace UA.Web.UI.Areas.Faculty.Models;
public sealed class FacultyForm
{
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
    [Required, StringLength(30, MinimumLength = 1), RegularExpression(@"^[A-Za-z0-9_-]+$")]
    public string EmployeeRef { get; set; } = "";
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, StringLength(50)] public string Designation { get; set; } = "";
    [Required] public string Type { get; set; } = "Full-time";
    [Required] public string Status { get; set; } = "Active";
    [StringLength(255)] public string? Qualifications { get; set; }
    [StringLength(20)] public string? Phone { get; set; }
    [EmailAddress, StringLength(100)] public string? Email { get; set; }
    public IFormFile? Photo { get; set; }
    public FacultySaveDto ToDto() => new(DepartmentId, EmployeeRef, FullName, Designation,
        Type, Status, Qualifications, Phone, Email);
}
public sealed class MentorAssignForm
{
    [Range(1,int.MaxValue)] public int DepartmentId { get; set; }
    [Range(1,int.MaxValue)] public int FacultyId { get; set; }
    [Range(1,int.MaxValue)] public int BatchId { get; set; }
    public int? SectionId { get; set; }
}
