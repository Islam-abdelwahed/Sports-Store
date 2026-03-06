using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class ProductAdminVM
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be 0 or greater")]
        public int StockQuantity { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<CategoryVM> Categories { get; set; } = [];
    }
}
