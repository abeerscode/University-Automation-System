namespace UA.Entities;

/// <summary>Roles held by users, with department scope.</summary>
/// <remarks>Requirements: FR-SEC-03, FR-SEC-04</remarks>
[Table("User_Role")]
[PrimaryKey(nameof(UserId), nameof(RoleId))]
public class UserRole
{
    /// <summary>User</summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Role</summary>
    [Column("role_id")]
    public int RoleId { get; set; }

    /// <summary>Department scope</summary>
    [Column("department_id")]
    public int? DepartmentId { get; set; }

    // Navigation
    [ForeignKey(nameof(UserId))]
    public virtual UserAccount User { get; set; } = null!;
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }
}
