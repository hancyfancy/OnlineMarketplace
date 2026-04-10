namespace Demo.Domain.Models;

public class ProductVariantInput
{
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Condition { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public bool IsAvailable { get; set; }
    public int UnitsInStock { get; set; }
    public int UnitsReserved { get; set; }
    public string? ImageUrl { get; set; }
    public int SortIndex { get; set; }
}
