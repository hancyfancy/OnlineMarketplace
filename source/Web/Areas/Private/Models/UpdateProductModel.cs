using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Private.Models
{
    public class UpdateProductModel
    {
        public Guid ProductGuid { get; set; }
        public Guid SellerGuid { get; set; }

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

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<UpdateVariantModel> Variants { get; set; } = [];

        public List<SelectListItem> CategoryOptions { get; set; } = [];
    }

    public class UpdateVariantModel
    {
        public Guid? VariantGuid { get; set; }

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

        [Display(Name = "Units In Stock")]
        public int UnitsInStock { get; set; }

        [Display(Name = "Units Reserved")]
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
