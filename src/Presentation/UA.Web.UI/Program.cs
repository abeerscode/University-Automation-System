using Microsoft.EntityFrameworkCore;
using UniversityAutomation.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var universityConnectionString = Environment.GetEnvironmentVariable("UNIVERSITY_DB")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(universityConnectionString))
{
    throw new InvalidOperationException(
        "Database connection string is missing. Set UNIVERSITY_DB or ConnectionStrings:DefaultConnection.");
}
builder.Services.AddDbContext<UniversityDbContext>(options =>
    options.UseSqlServer(universityConnectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
