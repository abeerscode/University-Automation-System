namespace UA.Entities;

/// <summary>Receipt issued for a payment.</summary>
/// <remarks>Requirements: FR-FIN-06, UR-02</remarks>
[Table("Receipt")]
[Index(nameof(PaymentId), IsUnique = true, Name = "UQ_Receipt_payment_id")]
[Index(nameof(ReceiptNo), IsUnique = true, Name = "UQ_Receipt_receipt_no")]
public class Receipt
{
    /// <summary>Receipt identifier</summary>
    [Key]
    [Column("receipt_id")]
    public int ReceiptId { get; set; }

    /// <summary>Payment</summary>
    [Column("payment_id")]
    public int PaymentId { get; set; }

    /// <summary>Receipt number</summary>
    [Required, MaxLength(30)]
    [Column("receipt_no")]
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>Time of issue</summary>
    [Column("issued_at")]
    public DateTime IssuedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(PaymentId))]
    public virtual Payment Payment { get; set; } = null!;
}
