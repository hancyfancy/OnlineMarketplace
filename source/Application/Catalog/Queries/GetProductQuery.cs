using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Queries;

public class GetProductQuery : IQuery<Result<GetProductDto>>
{
    public GetProductQuery(Guid productGuid)
    {
        ProductGuid = productGuid;
    }

    public Guid ProductGuid { get; }
}
