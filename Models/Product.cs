using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Product
    {

        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = [];

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
