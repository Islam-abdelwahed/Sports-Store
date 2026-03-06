using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Checkout(int id)
        {
            return View();
        }

        public IActionResult Checkout(int ce)
        {
            return View();
        }
    }
}
