using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class UserProfileUpdateCommandHandler : ICommandHandler<UserProfileUpdateCommand, Result>
    {
        private readonly IUserRepository _userRepository;

        public UserProfileUpdateCommandHandler(
            IUserRepository userRepository
        )
        {
            _userRepository = userRepository;
        }

        public async Task<Result> Handle(UserProfileUpdateCommand request, CancellationToken cancellationToken)
        {
            UserProfileUpdate userProfileUpdate = new UserProfileUpdate
            {
                FirstName = request.Model.FirstName,
                LastName = request.Model.LastName,
                PhoneNumber = request.Model.PhoneNumber,
                Role = request.Model.Role
            };

            return await _userRepository.UpdateAsync(request.Username, userProfileUpdate);
        }
    }
}
