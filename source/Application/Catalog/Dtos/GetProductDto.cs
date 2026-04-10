namespace Demo.Application.Catalog.Dtos;

public class GetProductDto
{
    public Guid ProductGuid { get; set; }
    public Guid SellerGuid { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public Guid CategoryGuid { get; set; }
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public string? SearchKeywords { get; set; }
    public bool IsDigital { get; set; }
    public string? ShippingType { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? BasePrice { get; set; }
    public bool IsActive { get; set; }
    public List<GetProductVariantDto> Variants { get; set; } = [];
}

public class GetProductVariantDto
{
    public Guid VariantGuid { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Condition { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int UnitsInStock { get; set; }
    public int UnitsReserved { get; set; }
    public bool IsAvailable => (UnitsInStock - UnitsReserved) > 0;
    public string? ImageUrl { get; set; }
    public int SortIndex { get; set; }
}
