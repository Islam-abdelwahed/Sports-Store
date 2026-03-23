using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Repositories;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController(IRepository<Order> orderRepo) : Controller
    {
        private readonly IRepository<Order> orderRepo = orderRepo;

        // GET: Admin/Orders
        public async Task<IActionResult> Index(OrderFilterOptions? filters)
        {
            filters ??= new OrderFilterOptions { SortBy = "OrderDate", SortOrder = "desc" };

            // Validate page size
            if (filters.PageSize < 10 || filters.PageSize > 100)
                filters.PageSize = 25;
            if (filters.PageNumber < 1)
                filters.PageNumber = 1;

            var query = orderRepo.Query().Include(o => o.User).Include(o => o.OrderItems);

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
            query = filters.SortBy?.ToLower() switch
            {
                "ordernumber" => filters.SortOrder == "desc"
                    ? query.OrderByDescending(o => o.OrderNumber)
                    : query.OrderBy(o => o.OrderNumber),
                "totalamount" => filters.SortOrder == "desc"
                    ? query.OrderByDescending(o => o.TotalAmount)
                    : query.OrderBy(o => o.TotalAmount),
                "status" => filters.SortOrder == "desc"
                    ? query.OrderByDescending(o => o.Status)
                    : query.OrderBy(o => o.Status),
                _ => filters.SortOrder == "desc"
                    ? query.OrderByDescending(o => o.OrderDate)
                    : query.OrderBy(o => o.OrderDate)
            };

            // Apply pagination
            var pagedResult = await query.PaginateAsync(filters.PageNumber, filters.PageSize);

            var viewModels = pagedResult.Items.Select(o => new OrderAdminVM
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = GetStatusText(o.Status),
                StatusValue = o.Status,
                UserEmail = o.User?.Email ?? "N/A"
            }).ToList();

            // Create view model with pagination
            var result = new PaginatedResult<OrderAdminVM>(
                viewModels,
                pagedResult.CurrentPage,
                pagedResult.PageSize,
                pagedResult.TotalItems)
            {
                Items = viewModels
            };

            // Pass filters to view
            ViewData["CurrentFilters"] = filters;

            return View(result);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await orderRepo.GetByIdAsync(id);
            if (order == null) return NotFound();

            var vm = new OrderDetailsVM
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

            return View(vm);
        }

        // GET: Admin/Orders/UpdateStatus/5
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var order = await orderRepo.GetByIdAsync(id);
            if (order == null) return NotFound();

            var vm = new OrderStatusVM
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                CurrentStatus = order.Status
            };

            return View(vm);
        }
        
        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(OrderStatusVM vm)
        {
            if (ModelState.IsValid)
            {
                var order = await orderRepo.GetByIdAsync(vm.OrderId);
                if (order == null) return NotFound();

                order.Status = vm.NewStatus;
                orderRepo.Update(order);
                await orderRepo.SaveAsync();

                return RedirectToAction(nameof(Details), new { id = vm.OrderId });
            }

            return View(vm);
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
