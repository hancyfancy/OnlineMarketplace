using MediatR;
using Demo.Application.Catalog.Abstractions;
using Demo.Application.Catalog.Commands;
using Demo.Application.Catalog.Dtos;
using Demo.Application.Catalog.Queries;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Services;

public class ProductService : IProductService
{
    private readonly IMediator _mediator;

    public ProductService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<ProductSearchPageDto>> SearchProductsAsync(ProductSearchQuery query)
        => await _mediator.Send(query);

    public async Task<Result> AddProductAsync(AddProductCommand command)
        => await _mediator.Send(command);

    public async Task<Result<List<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query)
        => await _mediator.Send(query);

    public async Task<Result<GetProductDto>> GetProductAsync(GetProductQuery query)
        => await _mediator.Send(query);

    public async Task<Result> UpdateProductAsync(UpdateProductCommand command)
        => await _mediator.Send(command);

    public async Task<Result> ArchiveProductAsync(ArchiveProductCommand command)
        => await _mediator.Send(command);
}
