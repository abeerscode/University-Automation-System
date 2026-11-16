using System.ComponentModel.DataAnnotations;
namespace UA.Web.UI.Areas.Website.Models;
public sealed class NoticeFormViewModel
{
    [Required, StringLength(200)] public string Title { get; set; } = "";
    [Required] public string Body { get; set; } = "";
    public int? DepartmentId { get; set; }
    public bool IsPublic { get; set; }
}
