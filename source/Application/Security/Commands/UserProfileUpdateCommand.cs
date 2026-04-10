using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class UserProfileUpdateCommand : ICommand<Result>
    {
        public UserProfileUpdateCommand(
            string username,
            UserProfileUpdateDto model
        )
        {
            Username = username;
            Model = model;
        }

        public string Username { get; private set; }

        public UserProfileUpdateDto Model { get; private set; }
    }
}
