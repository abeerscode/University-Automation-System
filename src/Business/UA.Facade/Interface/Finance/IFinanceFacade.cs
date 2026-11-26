using UA.Facade.DTO.Finance;
namespace UA.Facade.Interface.Finance;
public interface IFeeTypeFacade { Task<IReadOnlyList<FeeRateDto>> RatesAsync(CancellationToken ct = default); Task AddRateAsync(int departmentId, string name, decimal amount, DateTime effectiveFrom, CancellationToken ct = default); }
public interface IDueGenerator { Task GenerateSemesterAsync(string studentId, int termId, CancellationToken ct = default); Task GenerateLateFeeAsync(string studentId, int termId, CancellationToken ct = default); }
public interface IFeeClearanceChecker { Task<bool> IsClearAsync(string studentId, int? termId = null, CancellationToken ct = default); }
public interface IDueFacade { Task<IReadOnlyList<DueRow>> DuesAsync(int? departmentId = null, int? batchId = null, int? sectionId = null, string? status = null, CancellationToken ct = default); Task GenerateAsync(string studentId, int termId, CancellationToken ct = default); Task<FinanceLookup> LookupAsync(string permission, CancellationToken ct = default); }
public interface ILedgerFacade { Task<LedgerDto> StudentAsync(string studentId, CancellationToken ct = default); Task<LedgerDto> OwnAsync(CancellationToken ct = default); }
public interface IPaymentFacade { Task<int> CounterAsync(string studentId, decimal amount, int[]? dueIds, CancellationToken ct = default); Task<ReceiptDto> ReceiptAsync(int paymentId, bool own, CancellationToken ct = default); }
