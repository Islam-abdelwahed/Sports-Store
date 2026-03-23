namespace Project.Models
{
    /// <summary>
    /// Filter options specific to Orders
    /// </summary>
    public class OrderFilterOptions : FilterOptions
    {
        public int? OrderStatus { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortOrder { get; set; } = "desc"; // Orders sort descending by default
    }
}
