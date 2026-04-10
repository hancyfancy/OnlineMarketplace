using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Queries
{
    public class UserProfileQuery : IQuery<Result<UserProfileDto>>
    {
        public UserProfileQuery(
            string username
        )
        {
            Username = username;
        }

        public string Username { get; private set; }
    }
}
