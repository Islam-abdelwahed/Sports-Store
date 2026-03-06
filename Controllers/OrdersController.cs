using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Repositories;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Project.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Address> _addressRepo;
        private const string CartSessionKey = "ShoppingCart";

        public OrdersController(
            IRepository<Order> orderRepo,
            IRepository<Product> productRepo,
            IRepository<Address> addressRepo)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _addressRepo = addressRepo;
        }

        // GET: Orders/Index - User's order history
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderRepo.GetAllAsync();
            var userOrders = orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate);

            var viewModels = userOrders.Select(o => new OrderDetailsVM
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = GetStatusText(o.Status),
                TotalAmount = o.TotalAmount,
                Country = o.ShippingAddress?.Country ?? "",
                City = o.ShippingAddress?.City ?? "",
                Street = o.ShippingAddress?.Street ?? "",
                Zip = o.ShippingAddress?.Zip ?? "",
                Items = o.OrderItems.Select(oi => new OrderItemVM
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    SKU = oi.Product?.SKU ?? "",
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList()
            }).ToList();

            return View(viewModels);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderRepo.GetByIdAsync(id);
            
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            var viewModel = new OrderDetailsVM
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = GetStatusText(order.Status),
                TotalAmount = order.TotalAmount,
                Country = order.ShippingAddress?.Country ?? "",
                City = order.ShippingAddress?.City ?? "",
                Street = order.ShippingAddress?.Street ?? "",
                Zip = order.ShippingAddress?.Zip ?? "",
                Items = order.OrderItems.Select(oi => new OrderItemVM
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    SKU = oi.Product?.SKU ?? "",
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Orders/Checkout
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Please login to checkout.";
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new CheckoutVM
            {
                Items = cart,
                TotalAmount = cart.Sum(c => c.LineTotal)
            };

            return View(viewModel);
        }

        // POST: Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.Items = cart;
                model.TotalAmount = cart.Sum(c => c.LineTotal);
                return View(model);
            }

            try
            {
                // Create address
                var address = new Address
                {
                    UserId = userId,
                    Country = model.Country,
                    City = model.City,
                    Street = model.Street,
                    Zip = model.Zip,
                    IsDefault = model.SaveAddress
                };

                await _addressRepo.AddAsync(address);
                await _addressRepo.SaveAsync();

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    ShippingAddressId = address.AddressId,
                    OrderNumber = GenerateOrderNumber(),
                    Status = 0, // Pending
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Sum(c => c.LineTotal),
                    OrderItems = cart.Select(c => new OrderItem
                    {
                        ProductId = c.ProductId,
                        UnitPrice = c.UnitPrice,
                        Quantity = c.Quantity,
                        LineTotal = c.LineTotal
                    }).ToList()
                };

                await _orderRepo.AddAsync(order);
                await _orderRepo.SaveAsync();

                // Update product stock
                foreach (var item in cart)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                        _productRepo.Update(product);
                    }
                }
                await _productRepo.SaveAsync();

                // Clear cart
                HttpContext.Session.Remove(CartSessionKey);

                TempData["Success"] = "Order placed successfully!";
                return RedirectToAction(nameof(Details), new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error placing order. Please try again.";
                model.Items = cart;
                model.TotalAmount = cart.Sum(c => c.LineTotal);
                return View(model);
            }
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

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private static string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Processing",
                2 => "Shipped",
                3 => "Delivered",
                4 => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}
