using Microsoft.EntityFrameworkCore;
using Demo.Domain.Abstractions;
using Demo.Domain.Models;
using Demo.Domain.Primitives;
using Demo.Infrastructure.Catalog;

namespace Demo.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly IDbContextFactory<DemoCatalogDbContext> _dbContextFactory;

    public ProductRepository(IDbContextFactory<DemoCatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result> AddAsync(
        ProductAddInput product,
        List<ProductVariantInput> variants,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                var feeCategoryId = await context.ProductCategories
                    .AsNoTracking()
                    .Where(c => c.CategoryId == product.CategoryId)
                    .Select(c => c.FeeCategoryId)
                    .FirstOrDefaultAsync(cancellationToken);

                var now = DateTime.UtcNow;

                var productEntity = new ProductEntity
                {
                    ProductId = Guid.NewGuid(),
                    ProductGuid = Guid.NewGuid(),
                    SellerGuid = product.SellerGuid,
                    CategoryId = product.CategoryId,
                    FeeCategoryId = feeCategoryId,
                    Name = product.Name,
                    Slug = product.Slug,
                    ShortDescription = product.ShortDescription,
                    Brand = product.Brand,
                    SearchKeywords = product.SearchKeywords,
                    ShippingType = product.ShippingType,
                    ShippingCost = product.ShippingCost,
                    BasePrice = product.BasePrice,
                    IsActive = product.IsActive,
                    CreatedAt = now
                };

                context.Products.Add(productEntity);
                await context.SaveChangesAsync(cancellationToken);

                var variantEntities = variants.Select((v, i) => new ProductVariantEntity
                {
                    VariantId = Guid.NewGuid(),
                    VariantGuid = Guid.NewGuid(),
                    ProductId = productEntity.ProductId,
                    Sku = $"{product.Slug}-{(i + 1):D2}-{Guid.NewGuid().ToString("N")[..6]}",
                    Name = v.Name,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material,
                    Condition = v.Condition,
                    UnitPrice = v.UnitPrice,
                    CompareAtPrice = v.CompareAtPrice,
                    IsAvailable = v.UnitsInStock > v.UnitsReserved,
                    UnitsInStock = v.UnitsInStock,
                    UnitsReserved = v.UnitsReserved,
                    ImageUrl = v.ImageUrl,
                    SortIndex = v.SortIndex,
                    CreatedAt = now,
                    UpdatedAt = now
                }).ToList();

                if (variantEntities.Count > 0)
                {
                    context.ProductVariants.AddRange(variantEntities);
                    await context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return Result.Failure("AddFail", msg);
        }
    }

    public async Task<Result<List<CategoryItem>>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var categories = await context.ProductCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Level)
                .ThenBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryItem
                {
                    CategoryId = c.CategoryId,
                    CategoryGuid = c.CategoryGuid,
                    Name = c.Name,
                    Slug = c.Slug,
                    ParentCategoryId = c.ParentCategoryId,
                    Level = c.Level
                })
                .ToListAsync(cancellationToken);

            return Result.Success(categories);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<CategoryItem>>("GetCategoriesFail", ex.Message);
        }
    }

    public async Task<Result<Guid>> ResolveCategoryIdByGuidAsync(
        Guid categoryGuid,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var categoryId = await context.ProductCategories
                .AsNoTracking()
                .Where(c => c.CategoryGuid == categoryGuid)
                .Select(c => c.CategoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (categoryId == Guid.Empty)
                return Result.Failure<Guid>("CategoryNotFound", $"Category with guid {categoryGuid} not found.");

            return Result.Success(categoryId);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>("ResolveCategoryFail", ex.Message);
        }
    }

    public async Task<Result<(List<ProductSearchResult> Items, int TotalCount)>> SearchAsync(
        string? searchTerm,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        string? condition,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = context.Products.AsNoTracking().Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(term)) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)) ||
                    (p.SearchKeywords != null && p.SearchKeywords.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var matchingCategoryIds = context.ProductCategories
                    .Where(c => c.Slug == category && c.IsActive)
                    .Select(c => c.CategoryId);
                query = query.Where(p => matchingCategoryIds.Contains(p.CategoryId));
            }

            if (minPrice.HasValue)
                query = query.Where(p => p.BasePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(condition))
            {
                var productIdsWithCondition = context.ProductVariants
                    .Where(v => v.Condition == condition)
                    .Select(v => v.ProductId)
                    .Distinct();
                query = query.Where(p => productIdsWithCondition.Contains(p.ProductId));
            }

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.BasePrice),
                "price_desc" => query.OrderByDescending(p => p.BasePrice),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                "sold" => query.OrderByDescending(p => p.SoldCount),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.IsFeatured)
                                     .ThenByDescending(p => p.AverageRating)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(context.ProductCategories.AsNoTracking(),
                      p => p.CategoryId,
                      c => c.CategoryId,
                      (p, c) => new { Product = p, Category = c })
                .ToListAsync(cancellationToken);

            var productIds = products.Select(x => x.Product.ProductId).ToList();

            var variants = await context.ProductVariants
                .AsNoTracking()
                .Where(v => productIds.Contains(v.ProductId))
                .OrderBy(v => v.SortIndex)
                .ToListAsync(cancellationToken);

            var variantsByProduct = variants
                .GroupBy(v => v.ProductId)
                .ToDictionary(g => g.Key, g => new
                {
                    MinPrice = g.Where(v => v.IsAvailable).Min(v => (decimal?)v.UnitPrice),
                    ThumbnailUrl = g.OrderBy(v => v.SortIndex).FirstOrDefault()?.ImageUrl,
                    Condition = g.OrderBy(v => v.SortIndex).FirstOrDefault()?.Condition
                });

            var results = products.Select(x =>
            {
                variantsByProduct.TryGetValue(x.Product.ProductId, out var va);
                return new ProductSearchResult
                {
                    ProductGuid = x.Product.ProductGuid,
                    SellerGuid  = x.Product.SellerGuid,
                    Slug = x.Product.Slug,
                    Name = x.Product.Name,
                    Brand = x.Product.Brand,
                    ShortDescription = x.Product.ShortDescription,
                    BasePrice = x.Product.BasePrice,
                    SalePrice = x.Product.SalePrice,
                    IsOnSale = x.Product.IsOnSale,
                    DiscountPercentage = x.Product.DiscountPercentage,
                    AverageRating = x.Product.AverageRating,
                    ReviewCount = x.Product.ReviewCount,
                    SoldCount = x.Product.SoldCount,
                    IsInStock = x.Product.IsInStock,
                    IsLowStock = x.Product.IsLowStock,
                    IsFeatured = x.Product.IsFeatured,
                    ShippingType = x.Product.ShippingType,
                    ShippingCost = x.Product.ShippingCost,
                    IsDigital = false,
                    CategoryName = x.Category.Name,
                    CategorySlug = x.Category.Slug,
                    VariantMinPrice = va?.MinPrice,
                    ThumbnailUrl = va?.ThumbnailUrl,
                    Condition    = va?.Condition,
                    IsArchived   = false
                };
            }).ToList();

            return Result.Success((results, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Failure<(List<ProductSearchResult> Items, int TotalCount)>("SearchFail", ex.Message);
        }
    }

    public async Task<Result<ProductDetail>> GetByGuidAsync(
        Guid productGuid,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var product = await context.Products
                .AsNoTracking()
                .Where(p => p.ProductGuid == productGuid)
                .Join(context.ProductCategories.AsNoTracking(),
                      p => p.CategoryId,
                      c => c.CategoryId,
                      (p, c) => new { Product = p, Category = c })
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
                return Result.Failure<ProductDetail>("ProductNotFound", $"Product {productGuid} not found.");

            var variants = await context.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == product.Product.ProductId)
                .OrderBy(v => v.SortIndex)
                .ToListAsync(cancellationToken);

            var detail = new ProductDetail
            {
                ProductGuid = product.Product.ProductGuid,
                SellerGuid  = product.Product.SellerGuid,
                Name = product.Product.Name,
                Slug = product.Product.Slug,
                CategoryGuid = product.Category.CategoryGuid,
                ShortDescription = product.Product.ShortDescription,
                Brand = product.Product.Brand,
                SearchKeywords = product.Product.SearchKeywords,
                IsDigital = false,
                ShippingType = product.Product.ShippingType,
                ShippingCost = product.Product.ShippingCost,
                BasePrice = product.Product.BasePrice,
                IsActive = product.Product.IsActive,
                Variants = variants.Select(v => new ProductVariantDetail
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

            return Result.Success(detail);
        }
        catch (Exception ex)
        {
            return Result.Failure<ProductDetail>("GetProductFail", ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(
        ProductUpdateInput product,
        List<ProductVariantInput> variants,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await context.Products
                .Where(p => p.ProductGuid == product.ProductGuid)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing is null)
                return Result.Failure("ProductNotFound", $"Product {product.ProductGuid} not found or has been archived.");

            if (product.SellerGuid != Guid.Empty && existing.SellerGuid != product.SellerGuid)
                return Result.Failure("Forbidden", "You do not have permission to edit this product.");

            existing.CategoryId = product.CategoryId;
            existing.Name = product.Name;
            existing.Slug = product.Slug;
            existing.ShortDescription = product.ShortDescription;
            existing.Brand = product.Brand;
            existing.SearchKeywords = product.SearchKeywords;
            existing.ShippingType = product.ShippingType;
            existing.ShippingCost = product.ShippingCost;
            existing.BasePrice = product.BasePrice;
            var now = DateTime.UtcNow;
            existing.IsActive = product.IsActive;
            existing.UpdatedAt = now;

            var existingVariants = await context.ProductVariants
                .Where(v => v.ProductId == existing.ProductId)
                .ToListAsync(cancellationToken);

            context.ProductVariants.RemoveRange(existingVariants);

            var newVariants = variants.Select((v, i) => new ProductVariantEntity
            {
                VariantId = Guid.NewGuid(),
                VariantGuid = Guid.NewGuid(),
                ProductId = existing.ProductId,
                Sku = $"{product.Slug}-{(i + 1):D2}-{Guid.NewGuid().ToString("N")[..6]}",
                Name = v.Name,
                Color = v.Color,
                Size = v.Size,
                Material = v.Material,
                Condition = v.Condition,
                UnitPrice = v.UnitPrice,
                CompareAtPrice = v.CompareAtPrice,
                IsAvailable = v.UnitsInStock > v.UnitsReserved,
                UnitsInStock = v.UnitsInStock,
                UnitsReserved = v.UnitsReserved,
                ImageUrl = v.ImageUrl,
                SortIndex = v.SortIndex,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            context.ProductVariants.AddRange(newVariants);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("UpdateFail", ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<Result> ArchiveAsync(
        Guid productGuid,
        Guid archivedByGuid,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var strategy = context.Database.CreateExecutionStrategy();

            Result? earlyExit = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                var product = await context.Products
                    .Where(p => p.ProductGuid == productGuid)
                    .FirstOrDefaultAsync(cancellationToken);

                if (product is null)
                {
                    earlyExit = Result.Failure("ProductNotFound", $"Product {productGuid} not found.");
                    return;
                }

                var variants = await context.ProductVariants
                    .Where(v => v.ProductId == product.ProductId)
                    .ToListAsync(cancellationToken);

                var archivedAt = DateTime.UtcNow;
                var availableStock = variants.Sum(v => Math.Max(0, v.UnitsInStock - v.UnitsReserved));

                var productArchive = new ProductArchiveEntity
                {
                    ArchiveId        = Guid.NewGuid(),
                    ProductId        = product.ProductId,
                    ProductGuid      = product.ProductGuid,
                    SellerGuid       = product.SellerGuid,
                    CategoryId       = product.CategoryId,
                    FeeCategoryId    = product.FeeCategoryId,
                    Name             = product.Name,
                    Slug             = product.Slug,
                    ShortDescription = product.ShortDescription,
                    Brand            = product.Brand,
                    SearchKeywords   = product.SearchKeywords,
                    IsActive         = product.IsActive,
                    IsFeatured       = product.IsFeatured,
                    ShippingType     = product.ShippingType,
                    ShippingCost     = product.ShippingCost,
                    BasePrice        = product.BasePrice,
                    SalePrice        = product.SalePrice,
                    IsOnSale         = product.IsOnSale,
                    SoldCount        = product.SoldCount,
                    ReviewCount      = product.ReviewCount,
                    AverageRating    = product.AverageRating,
                    AvailableStock   = availableStock,
                    ArchivedAt       = archivedAt,
                    ArchivedByGuid   = archivedByGuid,
                    ArchiveReason    = "manual"
                };

                context.ArchivedProducts.Add(productArchive);
                await context.SaveChangesAsync(cancellationToken);

                if (variants.Count > 0)
                {
                    var variantArchives = variants.Select(v => new ProductVariantArchiveEntity
                    {
                        ArchiveId      = Guid.NewGuid(),
                        VariantId      = v.VariantId,
                        VariantGuid    = v.VariantGuid,
                        ProductId      = v.ProductId,
                        Sku            = v.Sku,
                        Name           = v.Name,
                        Color          = v.Color,
                        Size           = v.Size,
                        Material       = v.Material,
                        Condition      = v.Condition,
                        UnitsInStock   = v.UnitsInStock,
                        UnitsReserved  = v.UnitsReserved,
                        UnitPrice      = v.UnitPrice,
                        CompareAtPrice = v.CompareAtPrice,
                        ImageUrl       = v.ImageUrl,
                        SortIndex      = v.SortIndex,
                        ArchivedAt     = archivedAt,
                        ArchivedByGuid = archivedByGuid
                    }).ToList();

                    context.ArchivedProductVariants.AddRange(variantArchives);
                    context.ProductVariants.RemoveRange(variants);
                    await context.SaveChangesAsync(cancellationToken);
                }

                context.Products.Remove(product);
                await context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            });

            return earlyExit ?? Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("ArchiveFail", ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<Result<List<ProductSearchResult>>> SearchArchivedAsync(
        Guid sellerGuid,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = context.ArchivedProducts
                .AsNoTracking()
                .Where(p => p.SellerGuid == sellerGuid);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(term)) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)));
            }

            var products = await query
                .OrderByDescending(p => p.ArchivedAt)
                .Take(100)
                .Join(context.ProductCategories.AsNoTracking(),
                      p => p.CategoryId,
                      c => c.CategoryId,
                      (p, c) => new { Product = p, Category = c })
                .ToListAsync(cancellationToken);

            var productIds = products.Select(x => x.Product.ProductId).ToList();

            var variantThumbs = await context.ArchivedProductVariants
                .AsNoTracking()
                .Where(v => productIds.Contains(v.ProductId))
                .OrderBy(v => v.SortIndex)
                .ToListAsync(cancellationToken);

            var thumbByProduct = variantThumbs
                .GroupBy(v => v.ProductId)
                .ToDictionary(g => g.Key, g => g.First());

            var results = products.Select(x =>
            {
                thumbByProduct.TryGetValue(x.Product.ProductId, out var thumb);
                return new ProductSearchResult
                {
                    ProductGuid      = x.Product.ProductGuid,
                    SellerGuid       = x.Product.SellerGuid,
                    Slug             = x.Product.Slug,
                    Name             = x.Product.Name,
                    Brand            = x.Product.Brand,
                    ShortDescription = x.Product.ShortDescription,
                    BasePrice        = x.Product.BasePrice,
                    SalePrice        = x.Product.SalePrice,
                    IsOnSale         = x.Product.IsOnSale,
                    DiscountPercentage = x.Product.DiscountPercentage,
                    AverageRating    = x.Product.AverageRating,
                    ReviewCount      = x.Product.ReviewCount,
                    SoldCount        = x.Product.SoldCount,
                    IsInStock        = false,
                    IsLowStock       = false,
                    IsFeatured       = x.Product.IsFeatured,
                    ShippingType     = x.Product.ShippingType,
                    ShippingCost     = x.Product.ShippingCost,
                    IsDigital        = false,
                    CategoryName     = x.Category.Name,
                    CategorySlug     = x.Category.Slug,
                    VariantMinPrice  = null,
                    ThumbnailUrl     = thumb?.ImageUrl,
                    Condition        = thumb?.Condition,
                    IsArchived       = true
                };
            }).ToList();

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ProductSearchResult>>("SearchArchivedFail", ex.Message);
        }
    }
}
