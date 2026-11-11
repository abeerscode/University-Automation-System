namespace UniversityAutomation.Models;

/// <summary>Roles of the system.</summary>
/// <remarks>Requirements: FR-SEC-03</remarks>
[Table("Role")]
[Index(nameof(Name), IsUnique = true, Name = "UQ_Role_name")]
public class Role
{
    /// <summary>Role identifier</summary>
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    /// <summary>Role name</summary>
    [Required, MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Role is limited to a department</summary>
    [Column("department_scoped")]
    public bool DepartmentScoped { get; set; }

    // Navigation
    [InverseProperty(nameof(UserRole.Role))]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    [InverseProperty(nameof(RolePermission.Role))]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
