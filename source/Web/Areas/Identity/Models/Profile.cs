using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Identity.Models
{
    public class Profile
    {
        public Guid Id { get; set; }

        [BindNever]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = "";

        [BindNever]
        [Display(Name = "Role")]
        public string Role { get; set; } = "";

        public SellerProfile SellerProfile { get; set; } = new();

        public BuyerProfile BuyerProfile { get; set; } = new BuyerProfile();

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }
    }
}
