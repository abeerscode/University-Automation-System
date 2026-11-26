namespace UA.Facade.Interface.Finance;
public interface IFeeWaiverFacade { Task ApplyAsync(string studentId,int termId,decimal discount,int approvalId,CancellationToken ct=default); }
