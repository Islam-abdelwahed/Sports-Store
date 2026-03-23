namespace Project.Models
{
    /// <summary>
    /// Base class for filter options across all entities
    /// </summary>
    public class FilterOptions
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "Id";
        public string? SortOrder { get; set; } = "asc"; // asc or desc
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm);
    }
}
