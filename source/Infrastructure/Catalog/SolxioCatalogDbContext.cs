using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure.Catalog;

public class DemoCatalogDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductVariantEntity> ProductVariants { get; set; }
    public DbSet<ProductArchiveEntity> ArchivedProducts { get; set; }
    public DbSet<ProductVariantArchiveEntity> ArchivedProductVariants { get; set; }
    public DbSet<ProductCategoryEntity> ProductCategories { get; set; }
    public DbSet<FeeCategoryEntity> FeeCategories { get; set; }

    public DemoCatalogDbContext(DbContextOptions<DemoCatalogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ProductEntity>(b =>
        {
            b.ToTable("products", "catalog");
            b.HasKey(p => p.ProductId);
            b.Property(p => p.ProductId).HasColumnName("product_id");
            b.Property(p => p.ProductGuid).HasColumnName("product_guid");
            b.Property(p => p.SellerGuid).HasColumnName("seller_guid");
            b.Property(p => p.CategoryId).HasColumnName("category_id");
            b.Property(p => p.FeeCategoryId).HasColumnName("fee_category_id");
            b.Property(p => p.Name).HasColumnName("name");
            b.Property(p => p.Slug).HasColumnName("slug");
            b.Property(p => p.Description).HasColumnName("description");
            b.Property(p => p.ShortDescription).HasColumnName("short_description");
            b.Property(p => p.Brand).HasColumnName("brand");
            b.Property(p => p.IsActive).HasColumnName("is_active");
            b.Property(p => p.IsFeatured).HasColumnName("is_featured");
            b.Property(p => p.ShippingType).HasColumnName("shipping_type");
            b.Property(p => p.ShippingCost).HasColumnName("shipping_cost");
            b.Property(p => p.ShipsInternationally).HasColumnName("ships_internationally");
            b.Property(p => p.GspEligible).HasColumnName("gsp_eligible");
            b.Property(p => p.MetaTitle).HasColumnName("meta_title");
            b.Property(p => p.MetaDescription).HasColumnName("meta_description");
            b.Property(p => p.SearchKeywords).HasColumnName("search_keywords");
            b.Property(p => p.BasePrice).HasColumnName("base_price");
            b.Property(p => p.SalePrice).HasColumnName("sale_price");
            b.Property(p => p.IsOnSale).HasColumnName("is_on_sale");
            b.Property(p => p.TotalStock).HasColumnName("total_stock");
            b.Property(p => p.AvailableStock).HasColumnName("available_stock");
            b.Property(p => p.LowStockThreshold).HasColumnName("low_stock_threshold");
            b.Property(p => p.ViewCount).HasColumnName("view_count");
            b.Property(p => p.FavoriteCount).HasColumnName("favorite_count");
            b.Property(p => p.SoldCount).HasColumnName("sold_count");
            b.Property(p => p.ReviewCount).HasColumnName("review_count");
            b.Property(p => p.AverageRating).HasColumnName("average_rating");
            b.Property(p => p.CreatedAt).HasColumnName("created_at");
            b.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            b.Property(p => p.PublishedAt).HasColumnName("published_at");
            b.Property(p => p.CreatedByGuid).HasColumnName("created_by_guid");
            b.Property(p => p.UpdatedByGuid).HasColumnName("updated_by_guid");
            b.Property(p => p.IsInStock).HasColumnName("is_in_stock").HasConversion<int>().ValueGeneratedOnAddOrUpdate();
            b.Property(p => p.IsLowStock).HasColumnName("is_low_stock").HasConversion<int>().ValueGeneratedOnAddOrUpdate();
            b.Property(p => p.DiscountPercentage).HasColumnName("discount_percentage").ValueGeneratedOnAddOrUpdate();
        });

        builder.Entity<ProductVariantEntity>(b =>
        {
            b.ToTable("product_variants", "catalog");
            b.HasKey(v => v.VariantId);
            b.Property(v => v.VariantId).HasColumnName("variant_id");
            b.Property(v => v.VariantGuid).HasColumnName("variant_guid");
            b.Property(v => v.ProductId).HasColumnName("product_id");
            b.Property(v => v.Sku).HasColumnName("sku");
            b.Property(v => v.Name).HasColumnName("name");
            b.Property(v => v.Color).HasColumnName("color");
            b.Property(v => v.Size).HasColumnName("size");
            b.Property(v => v.Material).HasColumnName("material");
            b.Property(v => v.Condition).HasColumnName("condition");
            b.Property(v => v.UnitsInStock).HasColumnName("units_in_stock");
            b.Property(v => v.UnitsReserved).HasColumnName("units_reserved");
            b.Property(v => v.AvailableStock).HasColumnName("available_stock").ValueGeneratedOnAddOrUpdate();
            b.Property(v => v.IsAvailable).HasColumnName("is_available");
            b.Property(v => v.WeightGrams).HasColumnName("weight_grams");
            b.Property(v => v.PackageLengthCm).HasColumnName("package_length_cm");
            b.Property(v => v.PackageWidthCm).HasColumnName("package_width_cm");
            b.Property(v => v.PackageHeightCm).HasColumnName("package_height_cm");
            b.Property(v => v.UnitPrice).HasColumnName("unit_price");
            b.Property(v => v.CompareAtPrice).HasColumnName("compare_at_price");
            b.Property(v => v.CostPrice).HasColumnName("cost_price");
            b.Property(v => v.WeightOverrideGrams).HasColumnName("weight_override_grams");
            b.Property(v => v.ShippingSurcharge).HasColumnName("shipping_surcharge");
            b.Property(v => v.IsDigital).HasColumnName("is_digital");
            b.Property(v => v.RequiresShipping).HasColumnName("requires_shipping");
            b.Property(v => v.ImageUrl).HasColumnName("image_url");
            b.Property(v => v.SortIndex).HasColumnName("sort_index");
            b.Property(v => v.CreatedAt).HasColumnName("created_at");
            b.Property(v => v.UpdatedAt).HasColumnName("updated_at");
        });

        builder.Entity<ProductArchiveEntity>(b =>
        {
            b.ToTable("products_archive", "catalog");
            b.HasKey(p => p.ArchiveId);
            b.Property(p => p.ArchiveId).HasColumnName("archive_id").ValueGeneratedNever();
            b.Property(p => p.ProductId).HasColumnName("product_id");
            b.Property(p => p.ProductGuid).HasColumnName("product_guid");
            b.Property(p => p.SellerGuid).HasColumnName("seller_guid");
            b.Property(p => p.CategoryId).HasColumnName("category_id");
            b.Property(p => p.FeeCategoryId).HasColumnName("fee_category_id");
            b.Property(p => p.Name).HasColumnName("name");
            b.Property(p => p.Slug).HasColumnName("slug");
            b.Property(p => p.Description).HasColumnName("description");
            b.Property(p => p.ShortDescription).HasColumnName("short_description");
            b.Property(p => p.Brand).HasColumnName("brand");
            b.Property(p => p.IsActive).HasColumnName("is_active");
            b.Property(p => p.IsFeatured).HasColumnName("is_featured");
            b.Property(p => p.ShippingType).HasColumnName("shipping_type");
            b.Property(p => p.ShippingCost).HasColumnName("shipping_cost");
            b.Property(p => p.ShipsInternationally).HasColumnName("ships_internationally");
            b.Property(p => p.GspEligible).HasColumnName("gsp_eligible");
            b.Property(p => p.MetaTitle).HasColumnName("meta_title");
            b.Property(p => p.MetaDescription).HasColumnName("meta_description");
            b.Property(p => p.SearchKeywords).HasColumnName("search_keywords");
            b.Property(p => p.BasePrice).HasColumnName("base_price");
            b.Property(p => p.SalePrice).HasColumnName("sale_price");
            b.Property(p => p.IsOnSale).HasColumnName("is_on_sale");
            b.Property(p => p.TotalStock).HasColumnName("total_stock");
            b.Property(p => p.AvailableStock).HasColumnName("available_stock");
            b.Property(p => p.LowStockThreshold).HasColumnName("low_stock_threshold");
            b.Property(p => p.ViewCount).HasColumnName("view_count");
            b.Property(p => p.FavoriteCount).HasColumnName("favorite_count");
            b.Property(p => p.SoldCount).HasColumnName("sold_count");
            b.Property(p => p.ReviewCount).HasColumnName("review_count");
            b.Property(p => p.AverageRating).HasColumnName("average_rating");
            b.Property(p => p.OriginalCreatedAt).HasColumnName("original_created_at");
            b.Property(p => p.OriginalUpdatedAt).HasColumnName("original_updated_at");
            b.Property(p => p.OriginalPublishedAt).HasColumnName("original_published_at");
            b.Property(p => p.OriginalCreatedByGuid).HasColumnName("original_created_by_guid");
            b.Property(p => p.OriginalUpdatedByGuid).HasColumnName("original_updated_by_guid");
            b.Property(p => p.ArchivedAt).HasColumnName("archived_at");
            b.Property(p => p.ArchivedByGuid).HasColumnName("archived_by_guid");
            b.Property(p => p.ArchiveReason).HasColumnName("archive_reason");
            b.Property(p => p.ArchiveNotes).HasColumnName("archive_notes");
            b.Property(p => p.IsInStock).HasColumnName("is_in_stock").HasConversion<int>().ValueGeneratedOnAddOrUpdate();
            b.Property(p => p.IsLowStock).HasColumnName("is_low_stock").HasConversion<int>().ValueGeneratedOnAddOrUpdate();
            b.Property(p => p.DiscountPercentage).HasColumnName("discount_percentage").ValueGeneratedOnAddOrUpdate();
        });

        builder.Entity<ProductVariantArchiveEntity>(b =>
        {
            b.ToTable("product_variants_archive", "catalog");
            b.HasKey(v => v.ArchiveId);
            b.Property(v => v.ArchiveId).HasColumnName("archive_id").ValueGeneratedNever();
            b.Property(v => v.VariantId).HasColumnName("variant_id");
            b.Property(v => v.VariantGuid).HasColumnName("variant_guid");
            b.Property(v => v.ProductId).HasColumnName("product_id");
            b.Property(v => v.Sku).HasColumnName("sku");
            b.Property(v => v.Name).HasColumnName("name");
            b.Property(v => v.Color).HasColumnName("color");
            b.Property(v => v.Size).HasColumnName("size");
            b.Property(v => v.Material).HasColumnName("material");
            b.Property(v => v.Condition).HasColumnName("condition");
            b.Property(v => v.UnitsInStock).HasColumnName("units_in_stock");
            b.Property(v => v.UnitsReserved).HasColumnName("units_reserved");
            b.Property(v => v.AvailableStock).HasColumnName("available_stock").ValueGeneratedOnAddOrUpdate();
            b.Property(v => v.IsAvailable).HasColumnName("is_available");
            b.Property(v => v.WeightGrams).HasColumnName("weight_grams");
            b.Property(v => v.PackageLengthCm).HasColumnName("package_length_cm");
            b.Property(v => v.PackageWidthCm).HasColumnName("package_width_cm");
            b.Property(v => v.PackageHeightCm).HasColumnName("package_height_cm");
            b.Property(v => v.UnitPrice).HasColumnName("unit_price");
            b.Property(v => v.CompareAtPrice).HasColumnName("compare_at_price");
            b.Property(v => v.CostPrice).HasColumnName("cost_price");
            b.Property(v => v.WeightOverrideGrams).HasColumnName("weight_override_grams");
            b.Property(v => v.ShippingSurcharge).HasColumnName("shipping_surcharge");
            b.Property(v => v.IsDigital).HasColumnName("is_digital");
            b.Property(v => v.RequiresShipping).HasColumnName("requires_shipping");
            b.Property(v => v.ImageUrl).HasColumnName("image_url");
            b.Property(v => v.SortIndex).HasColumnName("sort_index");
            b.Property(v => v.OriginalCreatedAt).HasColumnName("original_created_at");
            b.Property(v => v.OriginalUpdatedAt).HasColumnName("original_updated_at");
            b.Property(v => v.ArchivedAt).HasColumnName("archived_at");
            b.Property(v => v.ArchivedByGuid).HasColumnName("archived_by_guid");
        });

        builder.Entity<ProductCategoryEntity>(b =>
        {
            b.ToTable("product_categories", "catalog");
            b.HasKey(c => c.CategoryId);
            b.Property(c => c.CategoryId).HasColumnName("category_id");
            b.Property(c => c.CategoryGuid).HasColumnName("category_guid");
            b.Property(c => c.Name).HasColumnName("name");
            b.Property(c => c.Slug).HasColumnName("slug");
            b.Property(c => c.IsActive).HasColumnName("is_active");
            b.Property(c => c.ParentCategoryId).HasColumnName("parent_category_id");
            b.Property(c => c.Level).HasColumnName("level");
            b.Property(c => c.SortOrder).HasColumnName("sort_order");
            b.Property(c => c.FeeCategoryId).HasColumnName("fee_category_id");
        });

        builder.Entity<FeeCategoryEntity>(b =>
        {
            b.ToTable("fee_categories", "catalog");
            b.HasKey(f => f.FeeCategoryId);
            b.Property(f => f.FeeCategoryId).HasColumnName("fee_category_id");
            b.Property(f => f.Code).HasColumnName("code");
            b.Property(f => f.Name).HasColumnName("name");
            b.Property(f => f.Description).HasColumnName("description");
            b.Property(f => f.Group).HasColumnName("group");
            b.Property(f => f.IsActive).HasColumnName("is_active");
            b.Property(f => f.SortOrder).HasColumnName("sort_order");
        });
    }
}
