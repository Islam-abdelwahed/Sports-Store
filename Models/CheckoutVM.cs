using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class CheckoutVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Required]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Zip Code")]
        public string Zip { get; set; } = string.Empty;

        public bool SaveAddress { get; set; } = false;
    }
}
