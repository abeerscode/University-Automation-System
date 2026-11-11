namespace UniversityAutomation.Models;

/// <summary>Online or counter payment, including reversing entries.</summary>
/// <remarks>Requirements: FR-FIN-05, FR-FIN-06, FR-FIN-11</remarks>
// CHECK constraint (to be configured in the DbContext): CHECK (student_id IS NOT NULL OR applicant_id IS NOT NULL)  -- Every due and payment has an owner (A-15)
[Table("Payment")]
[Index(nameof(GatewayTxnId), IsUnique = true, Name = "UQ_Payment_gateway_txn_id")]
public class Payment
{
    /// <summary>Payment identifier</summary>
    [Key]
    [Column("payment_id")]
    public int PaymentId { get; set; }

    /// <summary>Paying student</summary>
    [MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string? StudentId { get; set; }

    /// <summary>Paying applicant</summary>
    [Column("applicant_id")]
    public int? ApplicantId { get; set; }

    /// <summary>Gateway or Counter</summary>
    [Required, MaxLength(10)]
    [Column("method")]
    public string Method { get; set; } = string.Empty;

    /// <summary>Gateway transaction ID</summary>
    [MaxLength(60)]
    [Column("gateway_txn_id")]
    public string? GatewayTxnId { get; set; }

    /// <summary>Amount paid</summary>
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>Pending, Successful, Failed, Unconfirmed or Mismatch</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Time of payment</summary>
    [Column("paid_at")]
    public DateTime? PaidAt { get; set; }

    /// <summary>Accountant (counter payments)</summary>
    [Column("recorded_by")]
    public int? RecordedBy { get; set; }

    /// <summary>Payment that this entry reverses</summary>
    [Column("reverses_payment_id")]
    public int? ReversesPaymentId { get; set; }

    /// <summary>Reason of the reversal</summary>
    [MaxLength(255)]
    [Column("reversal_reason")]
    public string? ReversalReason { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student? Student { get; set; }
    [ForeignKey(nameof(ApplicantId))]
    public virtual Applicant? Applicant { get; set; }
    [ForeignKey(nameof(RecordedBy))]
    public virtual UserAccount? RecordedByUser { get; set; }
    [ForeignKey(nameof(ReversesPaymentId))]
    public virtual Payment? ReversesPayment { get; set; }
    [InverseProperty(nameof(Payment.ReversesPayment))]
    public virtual ICollection<Payment> Reversals { get; set; } = new List<Payment>();
    [InverseProperty(nameof(PaymentAllocation.Payment))]
    public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
    [InverseProperty(nameof(Receipt.Payment))]
    public virtual Receipt? Receipt { get; set; }
    [InverseProperty(nameof(GatewayCallback.Payment))]
    public virtual ICollection<GatewayCallback> GatewayCallbacks { get; set; } = new List<GatewayCallback>();
}
