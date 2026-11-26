using UA.Facade.DTO.Admission;
using UA.Facade.DTO.Student;
namespace UA.Facade.Interface.Admission;
public interface IAdmissionFacade
{
 Task<IReadOnlyList<AdmissionSeat>> DepartmentsAsync();
 Task<IReadOnlyList<CircularRow>> CircularsAsync(bool staff=false);
 Task CreateCircularAsync(CircularInput input);
 Task PublishCircularAsync(int id);
 Task UpdateCircularAsync(int id,DateTime close,DateTime test,string venue,DateTime result,DateTime deadline,string reason);
 Task CloseCircularAsync(int id,string reason);
 Task RegisterAsync(string email,string password,string mobile);
 Task<int> ApplyAsync(int circularId,int departmentId,ApplicantInput input,byte[] photo,string mime,byte[]? certificate);
 Task<IReadOnlyList<AdmissionRow>> ApplicationsAsync(int? circularId=null,bool staff=false);
 Task<AdmissionInvoice> InvoiceAsync(int id,bool staff=false);
 Task CreateEnrollmentInvoiceAsync(int id);
 Task<AdmitCardInfo> AdmitCardAsync(int id);
 Task<(byte[] Data,string Mime)> PhotoAsync(int id,bool staff=false);
 Task<IReadOnlyList<ApplicantDocumentRow>> DocumentsAsync(int id,bool staff=false);
 Task<(byte[] Data,string Mime)> DocumentAsync(int id,int documentId,bool staff=false);
 Task<IReadOnlyList<string>> ImportResultsAsync(int circularId,IReadOnlyList<TestResultInput> rows);
 Task PublishMeritAsync(int circularId);
 Task VerifyDocumentsAsync(int id);
 Task<StudentCreatedCredential> ConfirmAsync(int id);
 Task ReplaceAsync(int id,string reference);
 Task<AdmissionResultRow?> PublicResultAsync(int applicantId,int year,DateTime birth);
}
