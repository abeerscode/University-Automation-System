using UA.Integration.Import;
using Microsoft.Extensions.DependencyInjection;
using UA.Facade.Interface.Integration;
using UA.Integration.Security;
using UA.Integration.Messaging;
using UA.Integration.Storage;
using UA.Integration.Reporting;
namespace UA.Integration;
public static class DependencyInjection
{
    public static IServiceCollection AddUniversityIntegration(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddScoped<LocalFileStorage>();
        services.AddScoped<IPrivateReportStorage>(p => p.GetRequiredService<LocalFileStorage>());
        services.AddScoped<IApplicantStorage>(p => p.GetRequiredService<LocalFileStorage>());
        services.AddScoped<IFileStorage>(p => p.GetRequiredService<LocalFileStorage>());
        services.AddScoped<IStudentPhotoStorage>(p => p.GetRequiredService<LocalFileStorage>());
        services.AddScoped<IFacultyPhotoStorage>(p => p.GetRequiredService<LocalFileStorage>());
        services.AddSingleton<IStudentImportReader, StudentImportReader>();
        services.AddSingleton<IExcelWriter, CsvWriter>();
        services.AddSingleton<IAcademicWorkbookWriter, AcademicWorkbookWriter>();
        services.AddSingleton<IReceiptPdfWriter, ReceiptPdfWriter>();
        services.AddSingleton<IExamPdfWriter, ExamPdf>();
        return services;
    }
}
