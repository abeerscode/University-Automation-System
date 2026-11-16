namespace UA.Entities;

/// <summary>Permission matrix of the roles.</summary>
/// <remarks>Requirements: FR-SEC-03, SR-04</remarks>
[Table("Role_Permission")]
[PrimaryKey(nameof(RoleId), nameof(PermissionCode))]
public class RolePermission
{
    /// <summary>Role</summary>
    [Column("role_id")]
    public int RoleId { get; set; }

    /// <summary>Permission (module and action)</summary>
    [Required, MaxLength(50)]
    [Column("permission_code")]
    public string PermissionCode { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
}
