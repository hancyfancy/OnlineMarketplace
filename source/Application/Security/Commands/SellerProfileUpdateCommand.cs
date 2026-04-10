using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class SellerProfileUpdateCommand : ICommand<Result>
    {
        public SellerProfileUpdateCommand(Guid userGuid, SellerProfileUpdateDto model)
        {
            UserGuid = userGuid;
            Model = model;
        }

        public Guid UserGuid { get; private set; }
        public SellerProfileUpdateDto Model { get; private set; }
    }
}
