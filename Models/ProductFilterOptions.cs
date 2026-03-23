namespace Project.Models
{
    /// <summary>
    /// Filter options specific to Products
    /// </summary>
    public class ProductFilterOptions : FilterOptions
    {
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public bool? InStockOnly { get; set; }

        public override string ToString()
        {
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(SearchTerm))
                filters.Add($"SearchTerm={Uri.EscapeDataString(SearchTerm)}");
            if (CategoryId.HasValue)
                filters.Add($"CategoryId={CategoryId}");
            if (IsActive.HasValue)
                filters.Add($"IsActive={IsActive}");
            if (InStockOnly.HasValue)
                filters.Add($"InStockOnly={InStockOnly}");
            if (!string.IsNullOrEmpty(SortBy))
                filters.Add($"SortBy={SortBy}");
            if (!string.IsNullOrEmpty(SortOrder))
                filters.Add($"SortOrder={SortOrder}");
            filters.Add($"PageNumber={PageNumber}");
            filters.Add($"PageSize={PageSize}");

            return string.Join("&", filters);
        }
    }
}
