using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers
{
    public class CatalogController : Controller
    {
        public IActionResult Index()
        {
            return Content("dsd","plain/text");
        }

        public IActionResult Details(int id)
        {
            return View();
        }


    }
}
