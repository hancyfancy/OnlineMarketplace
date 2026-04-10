namespace Demo.Infrastructure.Catalog;

public class ProductVariantEntity
{
    public Guid VariantId { get; set; }
    public Guid VariantGuid { get; set; }
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Condition { get; set; }
    public int UnitsInStock { get; set; }
    public int UnitsReserved { get; set; }
    public int AvailableStock { get; set; }
    public bool IsAvailable { get; set; }
    public decimal? WeightGrams { get; set; }
    public decimal? PackageLengthCm { get; set; }
    public decimal? PackageWidthCm { get; set; }
    public decimal? PackageHeightCm { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? WeightOverrideGrams { get; set; }
    public decimal? ShippingSurcharge { get; set; }
    public bool IsDigital { get; set; }
    public bool RequiresShipping { get; set; }
    public string? ImageUrl { get; set; }
    public int SortIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
