using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using UA.Facade;
using UA.Facade.Interface.Administration;
using UA.Integration;
using UA.Framework.Security;
using UA.Web.UI.Infrastructure.Security;
using UA.Web.UI.Infrastructure.Authorization;
using UA.Web.UI.Infrastructure.Middleware;
using UA.Web.UI.Infrastructure.Filters;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    o.Filters.AddService<GlobalExceptionFilter>();
});
var universityConnectionString = Environment.GetEnvironmentVariable("UNIVERSITY_DB")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(universityConnectionString))
    throw new InvalidOperationException("Database connection string is missing. Set UNIVERSITY_DB or ConnectionStrings:DefaultConnection.");
builder.Services.AddUniversityBusiness(universityConnectionString);
builder.Services.AddUniversityIntegration();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/AccessDenied";
        o.Cookie.Name = "UAS.Auth"; o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(30); o.SlidingExpiration = true;
        o.Events.OnValidatePrincipal = async context =>
        {
            if (!int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ||
                !await context.HttpContext.RequestServices.GetRequiredService<IUserAccessFacade>().IsActiveAsync(id))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
});
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error/Index"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("areas", "{area:exists}/{controller=Users}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
if (builder.Configuration.GetValue<bool>("Uas:SeedOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IUniversityBootstrapper>().SeedAsync();
}
app.Run();
