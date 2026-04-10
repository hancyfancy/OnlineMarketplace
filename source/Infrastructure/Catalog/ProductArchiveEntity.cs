namespace Demo.Infrastructure.Catalog;

public class ProductArchiveEntity
{
    public Guid ArchiveId { get; set; }
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
    public DateTime? OriginalCreatedAt { get; set; }
    public DateTime? OriginalUpdatedAt { get; set; }
    public DateTime? OriginalPublishedAt { get; set; }
    public Guid? OriginalCreatedByGuid { get; set; }
    public Guid? OriginalUpdatedByGuid { get; set; }
    public DateTime ArchivedAt { get; set; }
    public Guid ArchivedByGuid { get; set; }
    public string ArchiveReason { get; set; } = "manual";
    public string? ArchiveNotes { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public decimal DiscountPercentage { get; set; }
}
