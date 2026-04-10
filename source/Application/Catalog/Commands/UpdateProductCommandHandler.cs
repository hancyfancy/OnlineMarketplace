using Demo.Application.Common.Abstractions;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;

namespace Demo.Application.Catalog.Commands;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryResult = await _productRepository.ResolveCategoryIdByGuidAsync(
            request.Product.CategoryGuid, cancellationToken);

        if (!categoryResult.IsSuccess)
            return Result.Failure(categoryResult.Error.Code, categoryResult.Error.Description);

        var product = new ProductUpdateInput
        {
            ProductGuid = request.Product.ProductGuid,
            SellerGuid  = request.SellerGuid,
            CategoryId = categoryResult.Value,
            Name = request.Product.Name,
            Slug = GenerateSlug(request.Product.Name),
            ShortDescription = request.Product.ShortDescription,
            Brand = request.Product.Brand,
            SearchKeywords = request.Product.SearchKeywords,
            IsDigital = request.Product.IsDigital,
            ShippingType = request.Product.ShippingType,
            ShippingCost = request.Product.ShippingCost,
            BasePrice = request.Product.BasePrice,
            IsActive = request.Product.IsActive
        };

        var variants = request.Product.Variants.Select((v, i) => new ProductVariantInput
        {
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
            SortIndex = v.SortIndex > 0 ? v.SortIndex : i
        }).ToList();

        return await _productRepository.UpdateAsync(product, variants, cancellationToken);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug + "-" + Guid.NewGuid().ToString("N")[..8];
    }
}
