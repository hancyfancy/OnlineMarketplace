using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Infrastructure.Identity;

namespace Demo.Infrastructure.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleRepository(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<RoleItem>> GetUserRolesAsync()
        {
            return await _roleManager.Roles
                .Where(r => r.RoleType == "user")
                .OrderBy(r => r.Priority)
                .Select(r => new RoleItem
                {
                    RoleGuid = r.Id,
                    Name = r.Name!,
                    Description = r.Description
                })
                .ToListAsync();
        }
    }
}
