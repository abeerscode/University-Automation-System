using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace UA.Web.UI.Areas.Portal.Models;
public sealed class ProfileForm
{
    [Required, StringLength(20)] public string Phone { get; set; } = "";
    [Required, StringLength(255)] public string Address { get; set; } = "";
    public IFormFile? Photo { get; set; }
}
