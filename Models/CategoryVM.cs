using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class CategoryVM
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name ="Category Name")]
        public string Name { get; set; } = String.Empty;

        [Display(Name="Parent Category")]
        public int? ParentCategoryId { get; set; }

        public string? ParentCategoryName { get; set; }

        public IEnumerable<CategoryVM> ParentCategories { get; set; } = [];
    }
}
