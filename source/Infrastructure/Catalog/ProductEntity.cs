namespace Demo.Infrastructure.Catalog;

public class ProductEntity
{
    public Guid ProductId { get; set; }
    public Guid ProductGuid { get; set; }
    public Guid SellerGuid { get; set; }
    public Guid CategoryId { get; set; }
    public Guid FeeCategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? ShippingType { get; set; }
    public decimal? ShippingCost { get; set; }
    public bool ShipsInternationally { get; set; }
    public bool GspEligible { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? SearchKeywords { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsOnSale { get; set; }
    public int TotalStock { get; set; }
    public int AvailableStock { get; set; }
    public int LowStockThreshold { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public int SoldCount { get; set; }
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid? CreatedByGuid { get; set; }
    public Guid? UpdatedByGuid { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public decimal DiscountPercentage { get; set; }
}
