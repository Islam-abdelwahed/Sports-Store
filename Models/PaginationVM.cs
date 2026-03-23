namespace Project.Models
{
    /// <summary>
    /// Generic paginated result wrapper for any entity type
    /// </summary>
    public class PaginatedResult<T> where T : class
    {
        public List<T> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        // Computed properties for view logic
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int FirstItemNumber => (CurrentPage - 1) * PageSize + 1;
        public int LastItemNumber => Math.Min(CurrentPage * PageSize, TotalItems);

        public PaginatedResult(List<T> items, int currentPage, int pageSize, int totalItems)
        {
            Items = items;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }

        public PaginatedResult() { }
    }
}
