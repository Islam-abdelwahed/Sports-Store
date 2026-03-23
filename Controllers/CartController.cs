using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Repositories;
using Newtonsoft.Json;

namespace Project.Controllers
{
    public class CartController : Controller
    {
        private readonly IRepository<Product> _productRepo;
        private const string CartSessionKey = "ShoppingCart";

        public CartController(IRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            var cartVM = await BuildCartViewModel(cart);
            return View(cartVM);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            
            if (product == null || !product.IsActive || product.StockQuantity < quantity)
            {
                TempData["Error"] = "Product not available.";
                return RedirectToAction("Index", "Catalog");
            }

            var cart = GetCart();
            
            // Check if product already in cart
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                // Don't exceed stock
                if (existingItem.Quantity > product.StockQuantity)
                {
                    existingItem.Quantity = product.StockQuantity;
                }
            }
            else
            {
                cart.Add(new CartItemVM
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    SKU = product.SKU,
                    UnitPrice = product.Price,
                    Quantity = quantity,
                    StockQuantity = product.StockQuantity
                });
            }

            SaveCart(cart);
            TempData["Success"] = "Product added to cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update(int itemId, int quantity)
        {
            if (quantity < 1)
            {
                return RedirectToAction(nameof(Remove), new { itemId });
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == itemId);
            
            if (item != null)
            {
                var product = await _productRepo.GetByIdAsync(itemId);
                if (product != null && quantity <= product.StockQuantity)
                {
                    item.Quantity = quantity;
                    SaveCart(cart);
                    TempData["Success"] = "Cart updated.";
                }
                else
                {
                    TempData["Error"] = "Requested quantity not available.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int itemId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == itemId);
            
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["Success"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/BuyNow - Add product to cart and redirect to checkout
        [HttpPost]
        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            var product = await _productRepo.GetByIdAsync(productId);

            if (product == null || !product.IsActive || product.StockQuantity < quantity)
            {
                TempData["Error"] = "Product not available.";
                return RedirectToAction("Index", "Catalog");
            }

            var cart = GetCart();

            // Clear cart and add only this product
            cart.Clear();
            cart.Add(new CartItemVM
            {
                ProductId = productId,
                ProductName = product.Name,
                SKU = product.SKU,
                UnitPrice = product.Price,
                Quantity = quantity,
                StockQuantity = product.StockQuantity
            });

            SaveCart(cart);
            return RedirectToAction("Checkout", "Orders");
        }

        private List<CartItemVM> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItemVM>();
            }
            return JsonConvert.DeserializeObject<List<CartItemVM>>(cartJson) ?? new List<CartItemVM>();
        }

        private void SaveCart(List<CartItemVM> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        private async Task<CartVM> BuildCartViewModel(List<CartItemVM> cart)
        {
            // Refresh stock quantities from database
            foreach (var item in cart)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    item.StockQuantity = product.StockQuantity;
                    item.UnitPrice = product.Price; // Update price if changed
                }
            }

            return new CartVM
            {
                Items = cart,
                TotalAmount = cart.Sum(c => c.LineTotal),
                TotalItems = cart.Sum(c => c.Quantity)
            };
        }
    }
}
