using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Commands
{
    public class BuyerProfileUpdateCommand : ICommand<Result>
    {
        public BuyerProfileUpdateCommand(Guid userGuid, BuyerProfileUpdateDto model)
        {
            UserGuid = userGuid;
            Model = model;
        }

        public Guid UserGuid { get; private set; }
        public BuyerProfileUpdateDto Model { get; private set; }
    }
}
