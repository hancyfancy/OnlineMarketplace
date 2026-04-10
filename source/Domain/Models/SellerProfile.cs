namespace Demo.Domain.Models
{
    public class SellerProfile
    {
        public Guid SellerProfileId { get; set; }
        public string UserId { get; set; } = "";

        public string? BusinessName { get; set; }
        public string BusinessType { get; set; } = "individual";
        public string? BusinessTaxId { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
        public string? BusinessRegistrationCountry { get; set; }
        public string? VatNumber { get; set; }

        public string SellerTier { get; set; } = "bronze";
        public string SellerStatus { get; set; } = "pending_verification";

        public int HandlingTimeDays { get; set; } = 2;
        public int ReturnPolicyDays { get; set; } = 30;
        public string RefundPolicy { get; set; } = "full";
        public string PayoutSchedule { get; set; } = "weekly";
    }
}
