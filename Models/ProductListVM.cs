namespace Project.Models
{
    public class ProductListVM
    {
        public List<ProductVM> Products { get; set; } = new();
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 12;
    }

    public class ProductVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
