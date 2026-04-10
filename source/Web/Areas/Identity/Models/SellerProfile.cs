using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Identity.Models
{
    public class SellerProfile
    {
        [Display(Name = "Business Name")]
        public string? BusinessName { get; set; }

        [Required]
        [Display(Name = "Business Type")]
        public string BusinessType { get; set; } = "individual";

        [Display(Name = "Business Tax ID")]
        public string? BusinessTaxId { get; set; }

        [Display(Name = "Registration Number")]
        public string? BusinessRegistrationNumber { get; set; }

        [Display(Name = "Registration Country")]
        [StringLength(2, MinimumLength = 2)]
        public string? BusinessRegistrationCountry { get; set; }

        [Display(Name = "VAT Number")]
        public string? VatNumber { get; set; }

        [Required]
        [Range(1, 30)]
        [Display(Name = "Handling Time (days)")]
        public int HandlingTimeDays { get; set; } = 2;

        [Required]
        [Range(0, 365)]
        [Display(Name = "Return Policy (days)")]
        public int ReturnPolicyDays { get; set; } = 30;

        [Required]
        [Display(Name = "Refund Policy")]
        public string RefundPolicy { get; set; } = "full";

        [Required]
        [Display(Name = "Payout Schedule")]
        public string PayoutSchedule { get; set; } = "weekly";

        public string SellerTier { get; set; } = "individual";
        public string SellerStatus { get; set; } = "pending_verification";
    }
}
