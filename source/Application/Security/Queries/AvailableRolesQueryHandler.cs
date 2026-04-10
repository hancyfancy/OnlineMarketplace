using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;

namespace Demo.Application.Security.Queries
{
    public class AvailableRolesQueryHandler : IQueryHandler<AvailableRolesQuery, List<RoleDto>>
    {
        private readonly IRoleRepository _roleRepository;

        public AvailableRolesQueryHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<RoleDto>> Handle(AvailableRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.GetUserRolesAsync();

            return roles.Select(r => new RoleDto
            {
                RoleGuid = r.RoleGuid,
                Name = r.Name,
                Description = r.Description
            }).ToList();
        }
    }
}
