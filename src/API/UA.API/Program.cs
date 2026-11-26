using UA.Facade;
using UA.Integration;
using UA.API.Infrastructure;
using UA.API.Workers;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var universityConnectionString = Environment.GetEnvironmentVariable("UNIVERSITY_DB")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(universityConnectionString))
    throw new InvalidOperationException("Database connection string is missing. Set UNIVERSITY_DB or ConnectionStrings:DefaultConnection.");
builder.Services.AddUniversityBusiness(universityConnectionString);
builder.Services.AddUniversityIntegration();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("sqlserver");
builder.Services.AddHostedService<NotificationDispatchWorker>();
builder.Services.AddHostedService<RegistrationWorker>();
builder.Services.AddHostedService<ExaminationWorker>();
builder.Services.AddHostedService<LifecycleWarningWorker>();
builder.Services.AddHostedService<AdmissionFinanceMaintenanceWorker>();
builder.Services.AddHostedService<AdmissionReportWorker>();
var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.MapHealthChecks("/health");
// Machine endpoints (signed gateway callbacks) will be added with the Finance integration.
// Do not expose MVC pages or anonymous database operations from UA.API.
app.Run();
