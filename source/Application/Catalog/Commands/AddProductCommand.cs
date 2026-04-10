using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Commands;

public class AddProductCommand : ICommand<Result>
{
    public AddProductCommand(Guid sellerGuid, AddProductDto product)
    {
        SellerGuid = sellerGuid;
        Product    = product;
    }

    public Guid SellerGuid { get; }
    public AddProductDto Product { get; }
}
