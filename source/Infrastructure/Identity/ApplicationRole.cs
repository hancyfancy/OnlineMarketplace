using Microsoft.AspNetCore.Identity;

namespace Demo.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<string>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
    }

    public string? Description { get; set; }
    public string RoleType { get; set; } = "custom";
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
