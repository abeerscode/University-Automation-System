using System.ComponentModel.DataAnnotations;
namespace UA.Web.UI.Areas.Administration.Models;
public sealed class RuleVersionForm
{
    [Range(1, int.MaxValue)] public int DepartmentId { get; set; } = 1;
    [DataType(DataType.Date)] public DateTime EffectiveFrom { get; set; } = DateTime.Today.AddMonths(2);
    [Required] public string Rows { get; set; } = "";
    public string CourseType { get; set; } = "Theory";
}
public sealed class TemplateForm
{
    public int TemplateId { get; set; }
    public string Code { get; set; } = "";
    public string? Subject { get; set; }
    [Required] public string Body { get; set; } = "";
    public bool Enabled { get; set; }
}
