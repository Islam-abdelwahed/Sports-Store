using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Repositories;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Project.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Index(OrderFilterOptions? filters)
        {
            filters ??= new OrderFilterOptions { SortBy = "OrderDate", SortOrder = "desc" };

            // Validate page size
            if (filters.PageSize < 10 || filters.PageSize > 100)
                filters.PageSize = 20;
            if (filters.PageNumber < 1)
                filters.PageNumber = 1;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _orderRepo.Query()
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId);

            // Apply search filter (order number)
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                query = query.Where(o => o.OrderNumber.ToLower().Contains(searchTerm));
            }

            // Apply status filter
            if (filters.OrderStatus.HasValue)
            {
                query = query.Where(o => o.Status == filters.OrderStatus);
            }

            // Apply date range filter
            if (filters.DateFrom.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date >= filters.DateFrom.Value.Date);
            }
            if (filters.DateTo.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date <= filters.DateTo.Value.Date);
            }

            // Apply sorting
            query = filters.SortOrder == "desc"
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate);

            // Apply pagination
            var pagedResult = await query.PaginateAsync(filters.PageNumber, filters.PageSize);

            var viewModels = pagedResult.Items.Select(o => new OrderDetailsVM
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = GetStatusText(o.Status),
                StatusCode = o.Status,
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

            var result = new PaginatedResult<OrderDetailsVM>(
                viewModels,
                pagedResult.CurrentPage,
                pagedResult.PageSize,
                pagedResult.TotalItems)
            {
                Items = viewModels
            };

            ViewData["CurrentFilters"] = filters;
            return View(result);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
                StatusCode = order.Status,
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

        // POST: Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _orderRepo.GetByIdAsync(id);

            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            // Check if order is cancellable (Pending or Processing only)
            if (order.Status != 0 && order.Status != 1)
            {
                TempData["Error"] = "This order cannot be cancelled. Orders can only be cancelled when they are Pending or Processing.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                // Update order status to Cancelled (4)
                order.Status = 4;
                _orderRepo.Update(order);
                await _orderRepo.SaveAsync();

                // Restore product stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _productRepo.Update(product);
                    }
                }
                await _productRepo.SaveAsync();

                TempData["Success"] = "Order has been cancelled successfully. Stock quantities have been restored.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cancelling order. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
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
