using Demo.Application.Catalog.Commands;
using Demo.Application.Catalog.Dtos;
using Demo.Application.Catalog.Queries;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Abstractions;

public interface IProductService
{
    Task<Result<ProductSearchPageDto>> SearchProductsAsync(ProductSearchQuery query);
    Task<Result> AddProductAsync(AddProductCommand command);
    Task<Result<List<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query);
    Task<Result<GetProductDto>> GetProductAsync(GetProductQuery query);
    Task<Result> UpdateProductAsync(UpdateProductCommand command);
    Task<Result> ArchiveProductAsync(ArchiveProductCommand command);
}
