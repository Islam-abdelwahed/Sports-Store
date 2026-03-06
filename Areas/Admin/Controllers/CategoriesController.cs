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

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM vm)
        {
            if(ModelState.IsValid)
            {
                var category = new Category
                {
                    Name = vm.Name,
                    ParentCategoryId = vm.ParentCategoryId
                };

                await categoryRepo.AddAsync(category);
                await categoryRepo.SaveAsync();

                return RedirectToAction(nameof(Index));
            }

            vm.ParentCategories = await GetParentCategoriesAsync();
            return View(vm);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var vm = new CategoryVM
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategories = await GetParentCategoriesAsync(category.CategoryId)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryVM vm) 
        {
            if(ModelState.IsValid)
            {
                var category = await categoryRepo.GetByIdAsync(vm.CategoryId);
                if (category == null) return NotFound();

                category.Name = vm.Name;
                category.ParentCategoryId = vm.ParentCategoryId;

                categoryRepo.Update(category);
                await categoryRepo.SaveAsync();

                return RedirectToAction(nameof(Index));
            }
            vm.ParentCategories = await GetParentCategoriesAsync(vm.CategoryId);
            return View(vm);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await categoryRepo.GetByIdAsync(id);
            if (category != null)
            {
                categoryRepo.Remove(category);
                await categoryRepo.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<CategoryVM>> GetParentCategoriesAsync(int? excludeId = null)
        {
            var all = await categoryRepo.GetAllAsync();
            return all.Where(c => c.CategoryId != excludeId).Select(c => new CategoryVM
            {
                CategoryId = c.CategoryId,
                Name = c.Name
            });
        }
    }
}
