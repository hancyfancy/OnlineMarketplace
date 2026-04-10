namespace Demo.Domain.Models;

public class ProductAddInput
{
    public Guid SellerGuid { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public string? SearchKeywords { get; set; }
    public bool IsDigital { get; set; }
    public string? ShippingType { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? BasePrice { get; set; }
    public bool IsActive { get; set; } = true;
}
