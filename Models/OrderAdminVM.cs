using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class OrderAdminVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusValue { get; set; }
        public string UserEmail { get; set; } = string.Empty;
    }

    public class OrderStatusVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CurrentStatus { get; set; }

        [Required]
        [Display(Name = "New Status")]
        public int NewStatus { get; set; }
    }
}
