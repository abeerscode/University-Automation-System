namespace UA.Facade.DTO.Finance;
public record FeeRateDto(int Id, int DepartmentId, string Name, string Unit, decimal Amount, DateTime EffectiveFrom);
public record DueRow(int Id, string StudentId, string StudentName, int DepartmentId, int BatchId, int SectionId, int? TermId, string Fee, decimal Amount, decimal Paid, DateTime Deadline, string Status) { public decimal Balance => Status is "Waived" or "Reversed" ? 0 : Amount - Paid; }
public record PaymentRow(int Id, decimal Amount, DateTime? PaidAt, string Method, string ReceiptNo);
public record LedgerDto(string StudentId, IReadOnlyList<DueRow> Dues, IReadOnlyList<PaymentRow> Payments) { public decimal Balance => Dues.Sum(d => d.Balance); }
public record ReceiptDto(string Number, string StudentId, DateTime IssuedAt, decimal Amount, IReadOnlyList<string> Allocations);
public record FinanceLookup(IReadOnlyList<KeyValuePair<int,string>> Departments, IReadOnlyList<KeyValuePair<int,string>> Batches, IReadOnlyList<KeyValuePair<int,string>> Sections, IReadOnlyList<KeyValuePair<int,string>> Terms);
