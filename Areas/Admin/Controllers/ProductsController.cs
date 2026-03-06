using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Repositories;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController(IRepository<Product> productRepo, IRepository<Category> categoryRepo) : Controller
    {
        private readonly IRepository<Product> productRepo = productRepo;
        private readonly IRepository<Category> categoryRepo = categoryRepo;

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var products = await productRepo.GetAllAsync();
            var viewModels = products.Select(p => new ProductVM
            {
                ProductId = p.ProductId,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "N/A"
            }).ToList();

            return View(viewModels);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            var vm = new ProductAdminVM
            {
                Categories = await GetCategoriesAsync()
            };
            return View(vm);
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductAdminVM vm)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = vm.Name,
                    SKU = vm.SKU,
                    Price = vm.Price,
                    StockQuantity = vm.StockQuantity,
                    IsActive = vm.IsActive,
                    CategoryId = vm.CategoryId
                };

                await productRepo.AddAsync(product);
                await productRepo.SaveAsync();

                return RedirectToAction(nameof(Index));
            }

            vm.Categories = await GetCategoriesAsync();
            return View(vm);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await productRepo.GetByIdAsync(id);
            if (product == null) return NotFound();

            var vm = new ProductAdminVM
            {
                ProductId = product.ProductId,
                Name = product.Name,
                SKU = product.SKU,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                Categories = await GetCategoriesAsync()
            };

            return View(vm);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductAdminVM vm)
        {
            if (ModelState.IsValid)
            {
                var product = await productRepo.GetByIdAsync(vm.ProductId);
                if (product == null) return NotFound();

                product.Name = vm.Name;
                product.SKU = vm.SKU;
                product.Price = vm.Price;
                product.StockQuantity = vm.StockQuantity;
                product.IsActive = vm.IsActive;
                product.CategoryId = vm.CategoryId;

                productRepo.Update(product);
                await productRepo.SaveAsync();

                return RedirectToAction(nameof(Index));
            }

            vm.Categories = await GetCategoriesAsync();
            return View(vm);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await productRepo.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await productRepo.GetByIdAsync(id);
            if (product != null)
            {
                productRepo.Remove(product);
                await productRepo.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<CategoryVM>> GetCategoriesAsync()
        {
            var categories = await categoryRepo.GetAllAsync();
            return categories.Select(c => new CategoryVM
            {
                CategoryId = c.CategoryId,
                Name = c.Name
            });
        }
    }
}
