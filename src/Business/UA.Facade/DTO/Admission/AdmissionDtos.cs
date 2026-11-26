namespace UA.Facade.DTO.Admission;
public record CircularRow(int Id,short Year,DateTime Open,DateTime Close,DateTime Test,string Venue,DateTime Result,DateTime Deadline,string Status,IReadOnlyList<AdmissionSeat> Seats);
public record AdmissionSeat(int DepartmentId,string Department,int Seats);
public record CircularInput(short Year,DateTime Open,DateTime Close,DateTime Test,string Venue,DateTime Result,DateTime Deadline,int[] DepartmentIds,short[] Seats);
public record ApplicantInput(string Name,DateTime Birth,string Gender,string Address,string Guardian,string GuardianPhone,string SscBoard,string SscRoll,decimal SscGpa,string HscBoard,string HscRoll,decimal HscGpa,bool Consent);
public record AdmissionRow(int Id,int ApplicantId,int CircularId,int DepartmentId,string Name,string Department,string Status,decimal? Marks,int? Rank,string? Selection,bool Verified,bool ApplicationPaid,bool EnrollmentPaid);
public record InvoiceLine(int DueId,string Fee,decimal Amount,decimal Paid,DateTime Deadline,string Status){public decimal Balance=>Status is "Waived" or "Reversed"?0:Math.Max(0,Amount-Paid);}
public record AdmissionInvoice(int ApplicationId,int ApplicantId,string Name,int Year,IReadOnlyList<InvoiceLine> Lines){public decimal Balance=>Lines.Sum(x=>x.Balance);}
public record AdmissionResultRow(int ApplicantId,int Year,string Department,int Rank,decimal Marks,string Selection);
public record TestResultInput(int Line,int ApplicantId,decimal Marks,int Rank);
public record ApplicantDocumentRow(int Id,string Type,string Mime);
public record AdmitCardInfo(int ApplicantId,string Name,string Department,DateTime Test,string Venue);
