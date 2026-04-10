using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Private.Models
{
    public class AddProductModel
    {
        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = "";

        [Required]
        [Display(Name = "Category")]
        public Guid CategoryGuid { get; set; }

        [Display(Name = "Short Description")]
        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Search Keywords")]
        public string? SearchKeywords { get; set; }

        [Display(Name = "Base Price")]
        [Range(0, 1_000_000, ErrorMessage = "Base price must be between 0 and 1,000,000.")]
        public decimal? BasePrice { get; set; }

        [Display(Name = "Shipping Type")]
        public string? ShippingType { get; set; }

        [Display(Name = "Shipping Cost")]
        [Range(0, 10_000, ErrorMessage = "Shipping cost must be between 0 and 10,000.")]
        public decimal? ShippingCost { get; set; }

        [Display(Name = "Digital Product")]
        public bool IsDigital { get; set; }

        public List<AddVariantModel> Variants { get; set; } = [];

        public List<SelectListItem> CategoryOptions { get; set; } = [];
    }

    public class AddVariantModel
    {
        [Display(Name = "Variant Name")]
        public string? Name { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Size")]
        public string? Size { get; set; }

        [Display(Name = "Material")]
        public string? Material { get; set; }

        [Required]
        [Display(Name = "Condition")]
        public string Condition { get; set; } = "new";

        [Required]
        [Range(0.01, 1_000_000, ErrorMessage = "Unit price must be greater than 0.")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Range(0, 1_000_000)]
        [Display(Name = "Compare At Price")]
        public decimal? CompareAtPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Stock count must be at least 1.")]
        [Display(Name = "Stock")]
        public int UnitsInStock { get; set; } = 1;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Reserved stock must be at least 10.")]
        [Display(Name = "Stock")]
        public int UnitsReserved { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Weight (g)")]
        [Range(0, 100_000)]
        public decimal? WeightGrams { get; set; }

        [Display(Name = "Length (cm)")]
        [Range(0, 10_000)]
        public decimal? PackageLengthCm { get; set; }

        [Display(Name = "Width (cm)")]
        [Range(0, 10_000)]
        public decimal? PackageWidthCm { get; set; }

        [Display(Name = "Height (cm)")]
        [Range(0, 10_000)]
        public decimal? PackageHeightCm { get; set; }

        public int SortIndex { get; set; }
    }
}
