using Demo.Domain.Models;

namespace Demo.Domain.Abstractions
{
    public interface IRoleRepository
    {
        Task<List<RoleItem>> GetUserRolesAsync();
    }
}
