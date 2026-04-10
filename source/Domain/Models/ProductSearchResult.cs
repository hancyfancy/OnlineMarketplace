namespace Demo.Domain.Models;

public class ProductSearchResult
{
    public Guid ProductGuid { get; set; }
    public Guid SellerGuid { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Brand { get; set; }
    public string? ShortDescription { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsOnSale { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsFeatured { get; set; }
    public string? ShippingType { get; set; }
    public decimal? ShippingCost { get; set; }
    public bool IsDigital { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategorySlug { get; set; } = "";
    public decimal? VariantMinPrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Condition { get; set; }
    public bool IsArchived { get; set; }
}
