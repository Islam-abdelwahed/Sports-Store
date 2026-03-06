using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add(int ProductId)
        {
            return View();
        }

        public IActionResult Update(int ItemId,int Quantity)
        {
            return View();
        }

        public IActionResult Remove(int ItemId)
        {
            return View();
        }
    }
}
