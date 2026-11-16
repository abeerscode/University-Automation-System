namespace UA.Entities;

/// <summary>Fee types and amounts per department.</summary>
/// <remarks>Requirements: FR-FIN-01, FR-FIN-02, BR-22</remarks>
[Table("Fee_Type")]
public class FeeType
{
    /// <summary>Fee type identifier</summary>
    [Key]
    [Column("fee_type_id")]
    public int FeeTypeId { get; set; }

    /// <summary>Department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Fee name</summary>
    [Required, MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Amount in Taka</summary>
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>Per application, semester, credit, course or request</summary>
    [Required, MaxLength(15)]
    [Column("unit")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>Date from which the rate applies</summary>
    [Column("effective_from", TypeName = "date")]
    public DateTime EffectiveFrom { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [InverseProperty(nameof(Due.FeeType))]
    public virtual ICollection<Due> Dues { get; set; } = new List<Due>();
}
