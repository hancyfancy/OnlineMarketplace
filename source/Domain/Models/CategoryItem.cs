namespace Demo.Domain.Models;

public class CategoryItem
{
    public Guid CategoryId { get; set; }
    public Guid CategoryGuid { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; }
}
