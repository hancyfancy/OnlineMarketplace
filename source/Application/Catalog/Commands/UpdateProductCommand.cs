using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Commands;

public class UpdateProductCommand : ICommand<Result>
{
    public UpdateProductCommand(Guid sellerGuid, UpdateProductDto product)
    {
        SellerGuid = sellerGuid;
        Product    = product;
    }

    public Guid SellerGuid { get; }
    public UpdateProductDto Product { get; }
}
