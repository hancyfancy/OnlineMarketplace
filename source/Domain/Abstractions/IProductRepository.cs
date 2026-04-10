using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Domain.Abstractions;

public interface IProductRepository
{
    Task<Result<(List<ProductSearchResult> Items, int TotalCount)>> SearchAsync(
        string? searchTerm,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        string? condition,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result> AddAsync(
        ProductAddInput product,
        List<ProductVariantInput> variants,
        CancellationToken cancellationToken = default);

    Task<Result<List<CategoryItem>>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> ResolveCategoryIdByGuidAsync(
        Guid categoryGuid,
        CancellationToken cancellationToken = default);

    Task<Result<ProductDetail>> GetByGuidAsync(
        Guid productGuid,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(
        ProductUpdateInput product,
        List<ProductVariantInput> variants,
        CancellationToken cancellationToken = default);

    Task<Result> ArchiveAsync(
        Guid productGuid,
        Guid archivedByGuid,
        CancellationToken cancellationToken = default);

    Task<Result<List<ProductSearchResult>>> SearchArchivedAsync(
        Guid sellerGuid,
        string? searchTerm,
        CancellationToken cancellationToken = default);
}
