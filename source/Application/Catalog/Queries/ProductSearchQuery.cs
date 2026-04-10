using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Queries;

public class ProductSearchQuery : IQuery<Result<ProductSearchPageDto>>
{
    public ProductSearchQuery(
        string? searchTerm,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        string? condition,
        string? sortBy,
        int page,
        int pageSize,
        Guid? sellerGuid = null)
    {
        SearchTerm = searchTerm;
        Category = category;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        Condition = condition;
        SortBy = sortBy;
        Page = page > 0 ? page : 1;
        PageSize = pageSize > 0 ? pageSize : 24;
        SellerGuid = sellerGuid;
    }

    public string? SearchTerm { get; }
    public string? Category { get; }
    public decimal? MinPrice { get; }
    public decimal? MaxPrice { get; }
    public string? Condition { get; }
    public string? SortBy { get; }
    public int Page { get; }
    public int PageSize { get; }
    public Guid? SellerGuid { get; }
}
