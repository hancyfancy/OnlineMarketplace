using Demo.Application.Catalog.Dtos;
using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Queries;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductDto>> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var result = await _productRepository.GetByGuidAsync(query.ProductGuid, cancellationToken);

        if (!result.IsSuccess)
            return Result.Failure<GetProductDto>(result.Error.Code, result.Error.Description);

        var p = result.Value;

        var dto = new GetProductDto
        {
            ProductGuid = p.ProductGuid,
            SellerGuid  = p.SellerGuid,
            Name = p.Name,
            Slug = p.Slug,
            CategoryGuid = p.CategoryGuid,
            ShortDescription = p.ShortDescription,
            Brand = p.Brand,
            SearchKeywords = p.SearchKeywords,
            IsDigital = p.IsDigital,
            ShippingType = p.ShippingType,
            ShippingCost = p.ShippingCost,
            BasePrice = p.BasePrice,
            IsActive = p.IsActive,
            Variants = p.Variants.Select(v => new GetProductVariantDto
            {
                VariantGuid = v.VariantGuid,
                Name = v.Name,
                Color = v.Color,
                Size = v.Size,
                Material = v.Material,
                Condition = v.Condition,
                UnitPrice = v.UnitPrice,
                CompareAtPrice = v.CompareAtPrice,
                UnitsInStock = v.UnitsInStock,
                UnitsReserved = v.UnitsReserved,
                ImageUrl = v.ImageUrl,
                SortIndex = v.SortIndex
            }).ToList()
        };

        return Result.Success(dto);
    }
}
