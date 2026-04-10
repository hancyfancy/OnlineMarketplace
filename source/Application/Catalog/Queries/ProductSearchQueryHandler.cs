using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Queries;

public class ProductSearchQueryHandler : IQueryHandler<ProductSearchQuery, Result<ProductSearchPageDto>>
{
    private readonly IProductRepository _productRepository;

    public ProductSearchQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductSearchPageDto>> Handle(ProductSearchQuery query, CancellationToken cancellationToken)
    {
        var result = await _productRepository.SearchAsync(
            query.SearchTerm,
            query.Category,
            query.MinPrice,
            query.MaxPrice,
            query.Condition,
            query.SortBy,
            query.Page,
            query.PageSize,
            cancellationToken);

        if (!result.IsSuccess)
            return Result.Failure<ProductSearchPageDto>(result.Error.Code, result.Error.Description);

        var (items, totalCount) = result.Value;

        static ProductSearchDto MapToDto(ProductSearchResult r) => new()
        {
            ProductGuid = r.ProductGuid,
            SellerGuid = r.SellerGuid,
            Slug = r.Slug,
            Name = r.Name,
            Brand = r.Brand,
            ShortDescription = r.ShortDescription,
            BasePrice = r.BasePrice,
            SalePrice = r.SalePrice,
            IsOnSale = r.IsOnSale,
            DiscountPercentage = r.DiscountPercentage,
            AverageRating = r.AverageRating,
            ReviewCount = r.ReviewCount,
            SoldCount = r.SoldCount,
            IsInStock = r.IsInStock,
            IsLowStock = r.IsLowStock,
            IsFeatured = r.IsFeatured,
            ShippingType = r.ShippingType,
            ShippingCost = r.ShippingCost,
            IsDigital = r.IsDigital,
            CategoryName = r.CategoryName,
            CategorySlug = r.CategorySlug,
            VariantMinPrice = r.VariantMinPrice,
            ThumbnailUrl = r.ThumbnailUrl,
            Condition = r.Condition,
            IsArchived = r.IsArchived
        };

        var liveItems = items.Select(MapToDto).ToList();

        List<ProductSearchDto> archivedItems = [];
        if (query.SellerGuid.HasValue)
        {
            var archivedResult = await _productRepository.SearchArchivedAsync(
                query.SellerGuid.Value, query.SearchTerm, cancellationToken);
            if (archivedResult.IsSuccess)
                archivedItems = archivedResult.Value.Select(MapToDto).ToList();
        }

        var dto = new ProductSearchPageDto
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            Items = [.. liveItems, .. archivedItems]
        };

        return Result.Success(dto);
    }
}
