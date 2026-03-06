using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = String.Empty;

        public int? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }
        public ICollection<Product> Products { get; set; } = [];
        public ICollection<Category> SubCategories { get; set; } = [];
    }
}
