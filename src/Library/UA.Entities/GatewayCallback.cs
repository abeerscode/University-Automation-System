namespace UniversityAutomation.Models;

/// <summary>Log of payment gateway callbacks for verification and reconciliation.</summary>
/// <remarks>Requirements: FR-FIN-12, NFR-INT-01 to NFR-INT-05</remarks>
[Table("Gateway_Callback")]
public class GatewayCallback
{
    /// <summary>Callback identifier</summary>
    [Key]
    [Column("callback_id")]
    public long CallbackId { get; set; }

    /// <summary>Gateway transaction ID</summary>
    [Required, MaxLength(60)]
    [Column("gateway_txn_id")]
    public string GatewayTxnId { get; set; } = string.Empty;

    /// <summary>Matched payment</summary>
    [Column("payment_id")]
    public int? PaymentId { get; set; }

    /// <summary>Amount reported by the gateway</summary>
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>Signature or token verified</summary>
    [Column("signature_valid")]
    public bool SignatureValid { get; set; }

    /// <summary>Matched, Mismatch, Rejected or Duplicate</summary>
    [Required, MaxLength(10)]
    [Column("match_status")]
    public string MatchStatus { get; set; } = string.Empty;

    /// <summary>Time received</summary>
    [Column("received_at")]
    public DateTime ReceivedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(PaymentId))]
    public virtual Payment? Payment { get; set; }
}
