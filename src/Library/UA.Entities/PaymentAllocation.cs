namespace UniversityAutomation.Models;

/// <summary>Split of a payment across dues.</summary>
/// <remarks>Requirements: FR-FIN-07</remarks>
[Table("Payment_Allocation")]
[PrimaryKey(nameof(PaymentId), nameof(DueId))]
public class PaymentAllocation
{
    /// <summary>Payment</summary>
    [Column("payment_id")]
    public int PaymentId { get; set; }

    /// <summary>Due settled</summary>
    [Column("due_id")]
    public int DueId { get; set; }

    /// <summary>Amount applied to the due</summary>
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    // Navigation
    [ForeignKey(nameof(PaymentId))]
    public virtual Payment Payment { get; set; } = null!;
    [ForeignKey(nameof(DueId))]
    public virtual Due Due { get; set; } = null!;
}
