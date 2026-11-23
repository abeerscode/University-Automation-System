using System.ComponentModel.DataAnnotations;
namespace UA.Web.UI.Areas.Student.Models;
public sealed class StudentEditForm
{
    [Required] public string StudentId { get; set; } = "";
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [DataType(DataType.Date)] public DateTime DateOfBirth { get; set; }
    [Required, StringLength(10)] public string Gender { get; set; } = "";
    [Required, StringLength(100)] public string GuardianName { get; set; } = "";
    [Required, StringLength(20)] public string GuardianPhone { get; set; } = "";
    [Required, StringLength(20)] public string Phone { get; set; } = "";
    [Required, StringLength(255)] public string Address { get; set; } = "";
    public string? ApprovalReference { get; set; }
}
public sealed class StatusChangeForm
{
    [Required] public string StudentId { get; set; } = "";
    [Required] public string Status { get; set; } = "Suspended";
    [DataType(DataType.Date)] public DateTime EffectiveDate { get; set; } = DateTime.Today;
    [Required, StringLength(255)] public string Reason { get; set; } = "";
    [Required] public string ApprovalReference { get; set; } = "";
}
public sealed class SectionChangeForm
{
    [Required] public string StudentId { get; set; } = "";
    [Range(1, int.MaxValue)] public int SectionId { get; set; }
    [Required, StringLength(255)] public string Reason { get; set; } = "";
    [Required] public string ApprovalReference { get; set; } = "";
}
