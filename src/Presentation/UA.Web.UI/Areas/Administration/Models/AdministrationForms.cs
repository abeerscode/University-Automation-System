using System.ComponentModel.DataAnnotations;
namespace UA.Web.UI.Areas.Administration.Models;
public sealed class UserFormViewModel
{
    [Required, StringLength(50)] public string Username { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    [EmailAddress] public string? Email { get; set; }
    public string? Mobile { get; set; }
    [Range(1, int.MaxValue)] public int RoleId { get; set; }
    public int? DepartmentId { get; set; }
}
public sealed class RoleFormViewModel { [Required, StringLength(50)] public string Name { get; set; } = ""; public bool DepartmentScoped { get; set; } }
public sealed class ApprovalFormViewModel
{
    [Required, StringLength(30)] public string Type { get; set; } = "";
    [Required, StringLength(15)] public string Status { get; set; } = "Approved";
    public int? DepartmentId { get; set; }
    [Range(1, int.MaxValue)] public int RequestorId { get; set; }
    public string? ScanPath { get; set; }
}
public sealed class ConfigFormViewModel
{
    [Required, StringLength(60)] public string Key { get; set; } = "";
    [Required, StringLength(255)] public string Value { get; set; } = "";
    public int? DepartmentId { get; set; }
    [Range(1, int.MaxValue)] public int EffectiveTermId { get; set; }
}
