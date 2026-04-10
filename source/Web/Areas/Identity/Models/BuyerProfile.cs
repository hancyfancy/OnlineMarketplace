using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Identity.Models
{
    public class BuyerProfile
    {
        [Display(Name = "Preferred Currency")]
        public string PreferredCurrency { get; set; } = "USD";

        public string BuyerLevel { get; set; } = "new";
        public decimal TrustScore { get; set; } = 5.00m;
        public decimal TotalPurchases { get; set; } = 0m;
        public decimal AvgOrderValue { get; set; } = 0m;
        public int TotalReturns { get; set; } = 0;
        public int OpenDisputes { get; set; } = 0;
        public decimal DisputeRate { get; set; } = 0m;
    }
}
