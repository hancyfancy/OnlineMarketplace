namespace Demo.Infrastructure.Catalog;

public class ProductCategoryEntity
{
    public Guid CategoryId { get; set; }
    public Guid CategoryGuid { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public bool IsActive { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public Guid FeeCategoryId { get; set; }
}
