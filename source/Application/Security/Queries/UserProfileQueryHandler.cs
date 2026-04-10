using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Queries
{
    public class UserProfileQueryHandler : IQueryHandler<UserProfileQuery, Result<UserProfileDto>>
    {
        private readonly IUserRepository _userRepository;

        public UserProfileQueryHandler(
            IUserRepository userRepository
        )
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserProfileDto>> Handle(UserProfileQuery request, CancellationToken cancellationToken)
        {
            Result<UserProfile> result = await _userRepository.ReadAsync(request.Username);

            if (result.IsFailure)
                return Result.Failure<UserProfileDto>(result.Error);

            return new UserProfileDto
            {
                Id = result.Value.Id,
                Email = result.Value.Email,
                FirstName = result.Value.FirstName,
                LastName = result.Value.LastName,
                PhoneNumber = result.Value.PhoneNumber,
                PasswordHash = result.Value.PasswordHash,
                Role = result.Value.Role,
                CreatedAt = result.Value.CreatedAt,
                UpdatedAt = result.Value.UpdatedAt,
                LastLoginAt = result.Value.LastLoginAt
            };
        }
    }
}
