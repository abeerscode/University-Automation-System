using UA.Entities;
using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Finance;
public interface IFeeCalculator { FeeType Rate(IEnumerable<FeeType> rates, string name, DateTime attendedTermStart); decimal Calculate(decimal rate, decimal quantity); }
public sealed class FeeCalculator : IFeeCalculator
{
    public FeeType Rate(IEnumerable<FeeType> rates, string name, DateTime attendedTermStart) => rates.Where(r => r.Name == name && r.EffectiveFrom.Date <= attendedTermStart.Date).OrderByDescending(r => r.EffectiveFrom).ThenByDescending(r => r.FeeTypeId).FirstOrDefault() ?? throw new BusinessRuleException("No effective rate for " + name);
    public decimal Calculate(decimal rate, decimal quantity) { if (rate < 0 || quantity <= 0) throw new BusinessRuleException("Invalid fee rate or quantity."); var amount = decimal.Round(rate * quantity, 2, MidpointRounding.AwayFromZero); if (amount > 99999999.99m) throw new BusinessRuleException("Fee exceeds database limit."); return amount; }
}
public interface ILateFeePolicy { bool IsLate(DateTime today, DateTime deadline); }
public sealed class LateFeePolicy : ILateFeePolicy { public bool IsLate(DateTime today, DateTime deadline) => today.Date > deadline.Date; }
public interface IPaymentAllocator { IReadOnlyList<(int DueId, decimal Amount)> Allocate(decimal amount, IEnumerable<(int Id, decimal Balance)> dues); }
public sealed class PaymentAllocator : IPaymentAllocator
{
    public IReadOnlyList<(int DueId, decimal Amount)> Allocate(decimal amount, IEnumerable<(int Id, decimal Balance)> dues)
    {
        var rows = dues.ToArray(); if (amount <= 0 || amount != decimal.Round(amount,2) || amount > 99999999.99m || amount > rows.Sum(x => x.Balance)) throw new BusinessRuleException("Payment must be positive, have at most two decimals and not exceed outstanding dues.");
        var result = new List<(int,decimal)>(); foreach (var row in rows) { var part = Math.Min(amount,row.Balance); if(part > 0) result.Add((row.Id,part)); amount -= part; if(amount == 0) break; } return result;
    }
}
public interface IReceiptNumberGenerator { string Generate(int paymentId, DateTime issued); }
public sealed class ReceiptNumberGenerator : IReceiptNumberGenerator { public string Generate(int paymentId, DateTime issued) => $"UAS-{issued:yyyy}-{paymentId:D10}"; }
public static class FeeNames
{
    public const string Registration = "Registration", Tuition = "Tuition", Retake = "Retake exam", Late = "Late fee";
    public static readonly IReadOnlyDictionary<string,string> Units = new Dictionary<string,string> { [Registration]="Semester", [Tuition]="Credit", [Retake]="Course", [Late]="Semester", ["Application"]="Application", ["Transcript"]="Request", ["Testimonial"]="Request", ["Certificate"]="Request", ["ID card"]="Request" };
}

public static class DueGenerationRules
{
    public static bool ShouldCreate(string status, IEnumerable<string> existingFeeNames, string name) => status=="Active" && !existingFeeNames.Contains(name);
}
public static class FeeClearanceRules
{
    public static bool IsClear(IEnumerable<UA.Facade.DTO.Finance.DueRow> dues,int? termId=null) => !dues.Any(d=>(termId==null || d.TermId==termId) && d.Balance>0 && d.Status is not ("Waived" or "Reversed"));
}
