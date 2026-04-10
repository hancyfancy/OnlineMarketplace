namespace Demo.Application.Catalog.Dtos;

public class AddProductDto
{
    public Guid CategoryGuid { get; set; }
    public string Name { get; set; } = "";
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public string? SearchKeywords { get; set; }
    public bool IsDigital { get; set; }
    public string? ShippingType { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? BasePrice { get; set; }
    public List<AddProductVariantDto> Variants { get; set; } = [];
}

public class AddProductVariantDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Condition { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int UnitsInStock { get; set; }
    public int UnitsReserved { get; set; }
    public string? ImageUrl { get; set; }
    public int SortIndex { get; set; }
}
