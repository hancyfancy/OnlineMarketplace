using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Commands;

public class ArchiveProductCommandHandler : ICommandHandler<ArchiveProductCommand, Result>
{
    private readonly IProductRepository _productRepository;

    public ArchiveProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        return await _productRepository.ArchiveAsync(request.ProductGuid, request.SellerGuid, cancellationToken);
    }
}
