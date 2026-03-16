using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Repositories;

namespace Project.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Category> _categoryRepo;

        public CatalogController(IRepository<Product> productRepo, IRepository<Category> categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index(int? categoryId, int page = 1, int pageSize = 12)
        {
            var products = await _productRepo.GetAllAsync();
            
            
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            
            products = products.Where(p => p.IsActive).ToList();

            // Calculate pagination
            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            
            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            
            string? categoryName = null;
            if (categoryId.HasValue)
            {
                var category = await _categoryRepo.GetByIdAsync(categoryId.Value);
                categoryName = category?.Name;
            }

            var viewModel = new ProductListVM
            {
                Products = pagedProducts.Select(p => new ProductVM
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "N/A"
                }).ToList(),
                CategoryId = categoryId,
                CategoryName = categoryName,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            
            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            var viewModel = new ProductVM
            {
                ProductId = product.ProductId,
                Name = product.Name,
                SKU = product.SKU,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "N/A"
            };

            return View(viewModel);
        }
    }
}
