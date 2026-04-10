namespace Web.Areas.Private.Models
{
    public class ProductSearchModel
    {
        public string? Q { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Condition { get; set; }
        public string SortBy { get; set; } = "relevance";
        public string ViewMode { get; set; } = "grid";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 24;

        public List<ProductSearchItemModel> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class ProductSearchItemModel
    {
        public Guid ProductGuid { get; set; }
        public Guid SellerGuid { get; set; }
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Brand { get; set; }
        public string? ShortDescription { get; set; }
        public decimal EffectivePrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public bool IsOnSale { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int SoldCount { get; set; }
        public bool IsInStock { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsDigital { get; set; }
        public string? ShippingType { get; set; }
        public decimal? ShippingCost { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Condition { get; set; }
        public string CategoryName { get; set; } = "";
        public bool IsArchived { get; set; }
    }
}
