using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Repositories;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController(IRepository<Order> orderRepo) : Controller
    {
        private readonly IRepository<Order> orderRepo = orderRepo;

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await orderRepo.GetAllAsync();
            var viewModels = orders.Select(o => new OrderAdminVM
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = GetStatusText(o.Status),
                StatusValue = o.Status,
                UserEmail = o.User?.Email ?? "N/A"
            }).OrderByDescending(o => o.OrderDate).ToList();

            return View(viewModels);
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
