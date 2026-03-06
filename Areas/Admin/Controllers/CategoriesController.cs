using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Repositories;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController(IRepository<Category> categoryRepo) : Controller
    {
        private readonly IRepository<Category> categoryRepo = categoryRepo;

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await categoryRepo.GetAllAsync();
            var viewModels = categories.Select(c => new CategoryVM
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId
            }).ToList();

            return View(viewModels);
        }
        
        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CategoryVM
            {
                ParentCategories = await GetParentCategoriesAsync()
            };
            return View(vm);
        }



        private async Task<IEnumerable<CategoryVM>> GetParentCategoriesAsync(int? excludeId = null)
        {
            throw new NotImplementedException();
        }
    }
}
