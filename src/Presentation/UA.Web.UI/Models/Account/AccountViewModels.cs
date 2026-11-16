using System.ComponentModel.DataAnnotations;
namespace UA.Web.UI.Models.Account;
public sealed class LoginViewModel
{
    [Required] public string Username { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public sealed class ForgotPasswordViewModel { [Required] public string Username { get; set; } = ""; }
public sealed class ResetPasswordViewModel
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Token { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public sealed class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)] public string OldPassword { get; set; } = "";
    [Required, DataType(DataType.Password)] public string NewPassword { get; set; } = "";
    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public sealed class VerifyCodeViewModel { [Required] public string Code { get; set; } = ""; }
