using Demo.Application.Common.Abstractions;
using Demo.Application.Security.Dtos;
using Demo.Domain.Primitives;

namespace Demo.Application.Security.Queries
{
    public class BuyerProfileQuery : IQuery<Result<BuyerProfileDto>>
    {
        public BuyerProfileQuery(Guid userGuid)
        {
            UserGuid = userGuid;
        }

        public Guid UserGuid { get; private set; }
    }
}
