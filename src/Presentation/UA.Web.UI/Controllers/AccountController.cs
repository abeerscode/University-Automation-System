using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UA.Facade.Interface.Administration;
using UA.Framework.Security;
using UA.Web.UI.Models.Account;
namespace UA.Web.UI.Controllers;
public sealed class AccountController(IAuthFacade auth, ICurrentUser current, IConfiguration configuration, IWebHostEnvironment environment) : Controller
{
    [AllowAnonymous, HttpGet] public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });
    [AllowAnonymous, HttpPost, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await auth.LoginAsync(model.Username, model.Password);
        if (!result.Succeeded)
        {
            if (result.Locked) return View("LockedOut");
            ModelState.AddModelError("", result.Message); return View(model);
        }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, result.UserId!.Value.ToString()),
            new Claim(ClaimTypes.Name, result.Username!) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }
    [HttpPost] public async Task<IActionResult> Logout()
    { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return RedirectToAction(nameof(Login)); }
    [AllowAnonymous, HttpGet] public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());
    [AllowAnonymous, HttpPost, EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var baseUrl = configuration["Uas:PublicBaseUrl"] ?? Environment.GetEnvironmentVariable("UAS_PUBLIC_BASE_URL");
        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = $"{Request.Scheme}://{Request.Host}";
        if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var url) &&
            (url.Scheme == "https" || environment.IsDevelopment() && url.Scheme == "http"))
            await auth.RequestResetAsync(model.Username, url.GetLeftPart(UriPartial.Authority));
        return View("ForgotPasswordConfirmation");
    }
    [AllowAnonymous, HttpGet] public IActionResult ResetPassword(string username, string token)
        => View(new ResetPasswordViewModel { Username = username, Token = token });
    [AllowAnonymous, HttpPost, EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await auth.ResetPasswordAsync(model.Username, model.Token, model.Password);
        if (!result.Succeeded) { ModelState.AddModelError("", result.Message); return View(model); }
        return View("ResetPasswordConfirmation");
    }
    [HttpGet] public IActionResult ChangePassword() => View(new ChangePasswordViewModel());
    [HttpPost] public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await auth.ChangePasswordAsync(current.UserId!.Value, model.OldPassword, model.NewPassword);
        if (!result.Succeeded) { ModelState.AddModelError("", result.Message); return View(model); }
        TempData["Message"] = result.Message; return RedirectToAction("Index", "Home");
    }
    [HttpGet] public IActionResult VerifyCode() => View(new VerifyCodeViewModel());
    [HttpPost] public async Task<IActionResult> SendVerification()
    {
        try { await auth.RequestVerificationAsync(current.UserId!.Value); TempData["Message"] = "Verification code sent if email is configured."; }
        catch (InvalidOperationException) { TempData["Error"] = "SMTP is not configured."; }
        return RedirectToAction(nameof(VerifyCode));
    }
    [HttpPost] public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await auth.VerifyCodeAsync(current.UserId!.Value, model.Code);
        if (!result.Succeeded) { ModelState.AddModelError("", result.Message); return View(model); }
        TempData["Message"] = result.Message; return RedirectToAction("Index", "Home");
    }
    [AllowAnonymous] public IActionResult AccessDenied() => View();
}
