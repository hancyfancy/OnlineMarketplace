namespace Demo.Application.Catalog.Dtos;

public class ProductSearchPageDto
{
    public List<ProductSearchDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
}
