namespace Project.Models
{
    /// <summary>
    /// Filter options specific to Categories
    /// </summary>
    public class CategoryFilterOptions : FilterOptions
    {
        public bool? IsMainCategoryOnly { get; set; }
    }
}
