using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Order
    {

        public int OrderId { get; set; }

        public string UserId { get; set; } = String.Empty;

        public int ShippingAddressId { get; set; }

        [Required]
        public string OrderNumber { get; set; } = String.Empty;

        public int Status { get; set; } = 0;

        public DateTime OrderDate { get; set; }  = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public AppUser User { get; set; } = null!;
        public Address ShippingAddress { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } =[];
    }
}
